using Forza.LastMile.TopPerformers.Application.Common.Interfaces;
using Forza.LastMile.TopPerformers.Domain.Common.Events;
using MediatR;

namespace Forza.LastMile.TopPerformers.Application.Features.Repartidores.Commands.AjustarMeta;

public sealed class AjustarMetaCommandHandler : IRequestHandler<AjustarMetaCommand>
{
    private const string IncidenciaDescripcionDefault = "Ajuste manual de meta por supervisor.";

    private readonly IRepartidorRendimientoRepository _repartidorRendimientoRepository;
    private readonly IPublisher _publisher;

    public AjustarMetaCommandHandler(
        IRepartidorRendimientoRepository repartidorRendimientoRepository,
        IPublisher publisher)
    {
        _repartidorRendimientoRepository = repartidorRendimientoRepository;
        _publisher = publisher;
    }

    public async Task Handle(AjustarMetaCommand request, CancellationToken cancellationToken)
    {
        var repartidor = await _repartidorRendimientoRepository.GetByIdAsync(request.RepartidorId);

        if (repartidor is null)
        {
            throw new KeyNotFoundException($"No existe un repartidor con ID {request.RepartidorId}.");
        }

        var metaOriginal = repartidor.MetaDiariaActual;

        repartidor.AjustarMetaPorIncidencia(IncidenciaDescripcionDefault, request.NuevaMeta);

        await _repartidorRendimientoRepository.UpdateAsync(repartidor);

        var metaAjustadaEvent = new MetaAjustadaEvent(
            repartidor.Id,
            metaOriginal,
            repartidor.MetaDiariaActual,
            repartidor.IncidenciaDescripcion ?? IncidenciaDescripcionDefault,
            DateTime.UtcNow);

        await _publisher.Publish(new MetaAjustadaNotification(metaAjustadaEvent), cancellationToken);
    }
}

public sealed record MetaAjustadaNotification(MetaAjustadaEvent EventData) : INotification;
