using Forza.LastMile.TopPerformers.Application.Common.Interfaces;
using Forza.LastMile.TopPerformers.Domain.Common.Events;
using MediatR;

namespace Forza.LastMile.TopPerformers.Application.Performers.Commands;

public sealed record AjustarMetaPorIncidenciaCommand(
	Guid RepartidorId,
	string IncidenciaDescripcion,
	int NuevaMetaSugerida) : IRequest<bool>;

public sealed class AjustarMetaPorIncidenciaCommandHandler : IRequestHandler<AjustarMetaPorIncidenciaCommand, bool>
{
	private const string MetaAjustadaSubject = "delivery.repartidor_rendimiento.meta_ajustada";

	private readonly IRepartidorRendimientoRepository _repartidorRendimientoRepository;
	private readonly IEventBus _eventBus;

	public AjustarMetaPorIncidenciaCommandHandler(
		IRepartidorRendimientoRepository repartidorRendimientoRepository,
		IEventBus eventBus)
	{
		_repartidorRendimientoRepository = repartidorRendimientoRepository;
		_eventBus = eventBus;
	}

	public async Task<bool> Handle(AjustarMetaPorIncidenciaCommand request, CancellationToken cancellationToken)
	{
		var repartidor = await _repartidorRendimientoRepository.GetByIdAsync(request.RepartidorId);

		if (repartidor is null)
		{
			return false;
		}

		var metaOriginal = repartidor.MetaDiariaActual;

		repartidor.AjustarMetaPorIncidencia(request.IncidenciaDescripcion, request.NuevaMetaSugerida);

		await _repartidorRendimientoRepository.UpdateAsync(repartidor);

		var metaAjustadaEvent = new MetaAjustadaEvent(
			repartidor.Id,
			metaOriginal,
			repartidor.MetaDiariaActual,
			request.IncidenciaDescripcion,
			DateTime.UtcNow);

		await _eventBus.PublishAsync(MetaAjustadaSubject, metaAjustadaEvent);

		return true;
	}
}
