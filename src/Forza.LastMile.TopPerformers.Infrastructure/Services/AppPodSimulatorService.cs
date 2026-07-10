using System.Globalization;
using Forza.LastMile.TopPerformers.Application.Common.Interfaces;
using Forza.LastMile.TopPerformers.Domain.Common.Events;
using Forza.LastMile.TopPerformers.Domain.Entities;
using Forza.LastMile.TopPerformers.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Forza.LastMile.TopPerformers.Infrastructure.Services;

public class AppPodSimulatorService : BackgroundService
{
	private const string Subject = "delivery.repartidor_rendimiento.transaccion_recibida";
	private static readonly string[] Vehiculos = ["Motocicleta", "Panel", "Automovil"];
	private readonly IServiceScopeFactory _serviceScopeFactory;
	private readonly ILogger<AppPodSimulatorService> _logger;
	private readonly Random _rnd = new();

	public AppPodSimulatorService(
		IServiceScopeFactory serviceScopeFactory,
		ILogger<AppPodSimulatorService> logger)
	{
		_serviceScopeFactory = serviceScopeFactory;
		_logger = logger;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		await SeedRepartidoresAsync(stoppingToken);

		while (!stoppingToken.IsCancellationRequested)
		{
			await using var scope = _serviceScopeFactory.CreateAsyncScope();
			var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
			var eventBus = scope.ServiceProvider.GetRequiredService<IEventBus>();

			var mesActual = DateTime.UtcNow.ToString("yyyy-MM", CultureInfo.InvariantCulture);
			var repartidores = await dbContext.RepartidoresRendimiento
				.Where(r => r.MesPeriodo == mesActual)
				.ToListAsync(stoppingToken);

			if (repartidores.Count > 0)
			{
				var cantidadASeleccionar = Math.Min(repartidores.Count, _rnd.Next(2, 4));
				var seleccionados = repartidores
					.OrderBy(_ => _rnd.Next())
					.Take(cantidadASeleccionar)
					.ToList();

				var actualizados = new List<RepartidorRendimiento>();

				foreach (var repartidor in seleccionados)
				{
					if (_rnd.Next(0, 10) > 3)
					{
						repartidor.IncrementarEntregasCompletadas();
						actualizados.Add(repartidor);
					}
				}

				if (actualizados.Count > 0)
				{
					await dbContext.SaveChangesAsync(stoppingToken);

					foreach (var repartidor in actualizados)
					{
						var evento = new TransaccionRecibidaEvent(
							repartidor.Id,
							repartidor.EntregasCompletadas,
							DateTime.UtcNow);

						await eventBus.PublishAsync(Subject, evento);
					}

					_logger.LogInformation(
						"Ciclo POD: seleccionados={Seleccionados}, actualizados={Actualizados}",
						seleccionados.Count,
						actualizados.Count);
				}
			}
			else
			{
				_logger.LogInformation("No hay repartidores para el periodo actual {MesPeriodo}.", mesActual);
			}

			await Task.Delay(4000, stoppingToken);
		}
	}

	private async Task SeedRepartidoresAsync(CancellationToken stoppingToken)
	{
		await using var scope = _serviceScopeFactory.CreateAsyncScope();
		var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

		var mesActual = DateTime.UtcNow.ToString("yyyy-MM", CultureInfo.InvariantCulture);
		var existentes = await dbContext.RepartidoresRendimiento
			.CountAsync(r => r.MesPeriodo == mesActual, stoppingToken);

		if (existentes >= 50)
		{
			return;
		}

		var aCrear = 50 - existentes;

		for (var i = 0; i < aCrear; i++)
		{
			var meta = _rnd.Next(10, 21);
			var repartidor = new RepartidorRendimiento(
				Guid.NewGuid(),
				$"Repartidor {existentes + i + 1:00}",
				Vehiculos[_rnd.Next(Vehiculos.Length)],
				Math.Round((decimal)(_rnd.NextDouble() * 4.0 + 1.0), 1),
				0,
				meta,
				meta,
				false,
				null,
				mesActual,
				DateTime.UtcNow);

			dbContext.RepartidoresRendimiento.Add(repartidor);
		}

		await dbContext.SaveChangesAsync(stoppingToken);

		_logger.LogInformation("Seed inicial de repartidores completado. Registros insertados: {Cantidad}", aCrear);
	}
}
