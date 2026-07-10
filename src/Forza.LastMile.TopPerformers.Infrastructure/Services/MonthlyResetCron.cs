using System.Globalization;
using Forza.LastMile.TopPerformers.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Forza.LastMile.TopPerformers.Infrastructure.Services;

public class MonthlyResetCron : BackgroundService
{
	// For tests, this interval can be changed to a shorter value (for example, minutes).
	private static readonly TimeSpan CheckInterval = TimeSpan.FromDays(1);

	private readonly IServiceScopeFactory _serviceScopeFactory;
	private readonly ILogger<MonthlyResetCron> _logger;

	public MonthlyResetCron(
		IServiceScopeFactory serviceScopeFactory,
		ILogger<MonthlyResetCron> logger)
	{
		_serviceScopeFactory = serviceScopeFactory;
		_logger = logger;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			var mesActual = DateTime.UtcNow.ToString("yyyy-MM", CultureInfo.InvariantCulture);

			using var scope = _serviceScopeFactory.CreateScope();
			var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

			var registrosDesfasados = await dbContext.RepartidoresRendimiento
				.Where(repartidor => repartidor.MesPeriodo != mesActual)
				.ToListAsync(stoppingToken);

			if (registrosDesfasados.Count > 0)
			{
				foreach (var repartidor in registrosDesfasados)
				{
					repartidor.ResetearParaNuevoMes(mesActual);
				}

				await dbContext.SaveChangesAsync(stoppingToken);

				_logger.LogInformation(
					"Monthly reset aplicado. Registros actualizados: {CantidadRegistros}. Nuevo mes_periodo: {MesPeriodo}",
					registrosDesfasados.Count,
					mesActual);
			}
			else
			{
				_logger.LogInformation("Monthly reset verificado sin cambios. mes_periodo actual: {MesPeriodo}", mesActual);
			}

			await Task.Delay(CheckInterval, stoppingToken);
		}
	}
}
