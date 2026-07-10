namespace Forza.LastMile.TopPerformers.Application.Common.Interfaces;

public interface INatsEventBus
{
    Task PublishAsync<T>(string subject, T eventData)
        where T : class;
}
