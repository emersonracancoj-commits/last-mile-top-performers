namespace Forza.LastMile.TopPerformers.Application.Common.Interfaces;

public interface IEventBus
{
	Task PublishAsync<T>(string subject, T eventData)
		where T : class;
}
