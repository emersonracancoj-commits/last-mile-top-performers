using Forza.LastMile.TopPerformers.Domain.Entities;

namespace Forza.LastMile.TopPerformers.Application.Common.Interfaces;

public interface IRepartidorRendimientoRepository
{
	Task<IReadOnlyCollection<RepartidorRendimiento>> GetTopPerformersAsync(string mesPeriodo);

	Task<RepartidorRendimiento?> GetByIdAsync(Guid id);

	Task UpdateAsync(RepartidorRendimiento repartidor);
}
