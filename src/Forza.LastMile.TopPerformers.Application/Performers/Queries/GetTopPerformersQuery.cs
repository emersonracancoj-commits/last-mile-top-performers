using System.Globalization;
using Forza.LastMile.TopPerformers.Application.Common.Interfaces;
using MediatR;

namespace Forza.LastMile.TopPerformers.Application.Performers.Queries;

public sealed record GetTopPerformersQuery : IRequest<List<RepartidorRendimientoDto>>;

public sealed record RepartidorRendimientoDto(
	Guid Id,
	string NombreRepartidor,
	string TipoVehiculo,
	decimal SatisfaccionScore,
	int EntregasCompletadas,
	int MetaDiariaOriginal,
	int MetaDiariaActual,
	bool IsMetaAjustada,
	string? IncidenciaDescripcion,
	string MesPeriodo,
	DateTime UltimaActualizacion);

public sealed class GetTopPerformersQueryHandler : IRequestHandler<GetTopPerformersQuery, List<RepartidorRendimientoDto>>
{
	private readonly IRepartidorRendimientoRepository _repartidorRendimientoRepository;

	public GetTopPerformersQueryHandler(IRepartidorRendimientoRepository repartidorRendimientoRepository)
	{
		_repartidorRendimientoRepository = repartidorRendimientoRepository;
	}

	public async Task<List<RepartidorRendimientoDto>> Handle(GetTopPerformersQuery request, CancellationToken cancellationToken)
	{
		var mesActual = DateTime.UtcNow.ToString("yyyy-MM", CultureInfo.InvariantCulture);
		var topPerformers = await _repartidorRendimientoRepository.GetTopPerformersAsync(mesActual);

		return topPerformers
			.Select(repartidor => new RepartidorRendimientoDto(
				repartidor.Id,
				repartidor.NombreRepartidor,
				repartidor.TipoVehiculo,
				repartidor.SatisfaccionScore,
				repartidor.EntregasCompletadas,
				repartidor.MetaDiariaOriginal,
				repartidor.MetaDiariaActual,
				repartidor.IsMetaAjustada,
				repartidor.IncidenciaDescripcion,
				repartidor.MesPeriodo,
				repartidor.UltimaActualizacion))
			.ToList();
	}
}
