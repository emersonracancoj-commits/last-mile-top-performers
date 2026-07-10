using Forza.LastMile.TopPerformers.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace Forza.LastMile.TopPerformers.Infrastructure.Messaging;

public class NatsEventBus : IEventBus, INatsEventBus
{
	private const string MetaAjustadaSubject = "delivery.repartidor_rendimiento.meta_ajustada";
	private const string TransaccionRecibidaSubject = "delivery.repartidor_rendimiento.transaccion_recibida";

	private readonly ILogger<NatsEventBus> _logger;

	public NatsEventBus(ILogger<NatsEventBus> logger)
	{
		_logger = logger;
	}

	public Task PublishAsync<T>(string subject, T eventData)
		where T : class
	{
		var normalizedSubject = NormalizeSubject(subject);

		_logger.LogInformation(
			"Publishing integration event to subject {Subject}. Payload: {@EventData}",
			normalizedSubject,
			eventData);

		return Task.CompletedTask;
	}

	private static string NormalizeSubject(string subject)
	{
		return subject switch
		{
			"meta_ajustada" => MetaAjustadaSubject,
			"transaccion_recibida" => TransaccionRecibidaSubject,
			_ => subject
		};
	}
}
