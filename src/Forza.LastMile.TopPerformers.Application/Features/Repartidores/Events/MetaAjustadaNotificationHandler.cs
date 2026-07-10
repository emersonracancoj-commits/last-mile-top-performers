using Forza.LastMile.TopPerformers.Application.Common.Interfaces;
using Forza.LastMile.TopPerformers.Application.Features.Repartidores.Commands.AjustarMeta;
using MediatR;

namespace Forza.LastMile.TopPerformers.Application.Features.Repartidores.Events;

public sealed class MetaAjustadaNotificationHandler : INotificationHandler<MetaAjustadaNotification>
{
    private const string MetaAjustadaSubject = "delivery.repartidor_rendimiento.meta_ajustada";

    private readonly INatsEventBus _natsEventBus;

    public MetaAjustadaNotificationHandler(INatsEventBus natsEventBus)
    {
        _natsEventBus = natsEventBus;
    }

    public async Task Handle(MetaAjustadaNotification notification, CancellationToken cancellationToken)
    {
        var evento = notification.EventData;
        await _natsEventBus.PublishAsync(MetaAjustadaSubject, evento);
    }
}
