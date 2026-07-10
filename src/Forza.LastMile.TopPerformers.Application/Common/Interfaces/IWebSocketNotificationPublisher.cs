namespace Forza.LastMile.TopPerformers.Application.Common.Interfaces;

public interface IWebSocketNotificationPublisher
{
	Task PublishNotificationAsync<T>(string message, T payload);
}
