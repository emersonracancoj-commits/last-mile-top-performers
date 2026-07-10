using Forza.LastMile.TopPerformers.Application.Common.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NATS.Client.Core;

namespace Forza.LastMile.TopPerformers.Infrastructure.Messaging;

public class NatsWebSocketBridge : BackgroundService
{
	private readonly INatsConnection _nats;
	private readonly IWebSocketNotificationPublisher _publisher;
	private readonly ILogger<NatsWebSocketBridge> _logger;

	// Temas NATS suscritos
	private const string MetaAjustadaTopic = "delivery.repartidor_rendimiento.meta_ajustada";
	private const string TransaccionRecibidaTopic = "delivery.repartidor_rendimiento.transaccion_recibida";
	private const string WildcardTopic = "delivery.repartidor_rendimiento.>";

	public NatsWebSocketBridge(
		INatsConnection nats,
		IWebSocketNotificationPublisher publisher,
		ILogger<NatsWebSocketBridge> logger)
	{
		_nats = nats;
		_publisher = publisher;
		_logger = logger;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		try
		{
			_logger.LogInformation("Starting NATS WebSocket Bridge...");
			_logger.LogInformation("Subscribed to NATS topic: {Topic}", WildcardTopic);

			await foreach (var message in _nats.SubscribeAsync<string>("delivery.repartidor_rendimiento.>", cancellationToken: stoppingToken))
			{
				if (string.IsNullOrWhiteSpace(message.Data))
				{
					continue;
				}

				await OnEventReceivedAsync(message.Data);
			}

			_logger.LogInformation("Stopping NATS WebSocket Bridge...");
		}
		catch (OperationCanceledException)
		{
			_logger.LogInformation("NATS WebSocket Bridge cancelled.");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error in NATS WebSocket Bridge");
			throw;
		}
	}

	/// <summary>
	/// Publicar evento a todos los clientes WebSocket conectados
	/// Llamado internamente cuando se reciben mensajes desde NATS
	/// </summary>
	public async Task OnEventReceivedAsync(string mensaje)
	{
		try
		{
			_logger.LogInformation(
				"NATS event received. Broadcasting to WebSocket clients. Payload: {Mensaje}",
				mensaje);

			await _publisher.PublishNotificationAsync("ReceiveEvent", mensaje);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error broadcasting event to WebSocket clients");
		}
	}

	private static string NormalizeEventName(string subject)
	{
		return subject switch
		{
			MetaAjustadaTopic => "meta_ajustada",
			TransaccionRecibidaTopic => "transaccion_recibida",
			_ => subject.Split('.').Last()
		};
	}
}
