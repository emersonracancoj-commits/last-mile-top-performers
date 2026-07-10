using Forza.LastMile.TopPerformers.Application.Common.Interfaces;
using Forza.LastMile.TopPerformers.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Forza.LastMile.TopPerformers.Infrastructure.Persistence;

public class RepartidorRendimientoRepository : IRepartidorRendimientoRepository
{
	private readonly ApplicationDbContext _applicationDbContext;

	public RepartidorRendimientoRepository(ApplicationDbContext applicationDbContext)
	{
		_applicationDbContext = applicationDbContext;
	}

	public async Task<RepartidorRendimiento?> GetByIdAsync(Guid id)
	{
		return await _applicationDbContext.RepartidoresRendimiento
			.FirstOrDefaultAsync(repartidor => repartidor.Id == id);
	}

	public async Task UpdateAsync(RepartidorRendimiento repartidor)
	{
		_applicationDbContext.Entry(repartidor).State = EntityState.Modified;
		await _applicationDbContext.SaveChangesAsync();
	}

	public async Task<IReadOnlyCollection<RepartidorRendimiento>> GetTopPerformersAsync(string mesPeriodo)
	{
		return await _applicationDbContext.RepartidoresRendimiento
			.Where(repartidor => repartidor.MesPeriodo == mesPeriodo)
			.OrderByDescending(repartidor => repartidor.EntregasCompletadas)
			.ToListAsync();
	}
}
