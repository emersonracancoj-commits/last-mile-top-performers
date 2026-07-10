using Forza.LastMile.TopPerformers.Application.Common.Interfaces;
using Forza.LastMile.TopPerformers.WebAPI.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Forza.LastMile.TopPerformers.WebAPI.Services;

public class NotificationPublisher : IWebSocketNotificationPublisher
{
	private readonly IHubContext<NotificationHub> _hubContext;
	private readonly ILogger<NotificationPublisher> _logger;

	public NotificationPublisher(
		IHubContext<NotificationHub> hubContext,
		ILogger<NotificationPublisher> logger)
	{
		_hubContext = hubContext;
		_logger = logger;
	}

	public async Task PublishNotificationAsync<T>(string message, T payload)
	{
		try
		{
			_logger.LogInformation(
				"Publishing websocket notification. Message: {Message}, Payload: {@Payload}",
				message,
				payload);

			await _hubContext.Clients.All.SendAsync(message, payload);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error publishing websocket notification");
			throw;
		}
	}
}
