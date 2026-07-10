using Forza.LastMile.TopPerformers.Application.Common.Interfaces;
using Forza.LastMile.TopPerformers.Infrastructure.Messaging;
using Forza.LastMile.TopPerformers.Infrastructure.Persistence;
using Forza.LastMile.TopPerformers.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NATS.Client.Core;

namespace Forza.LastMile.TopPerformers.Infrastructure;

public static class DependencyInjection
{
	public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

		services.AddDbContext<ApplicationDbContext>(options =>
			options.UseNpgsql(connectionString));

		services.AddSingleton<INatsConnection>(_ => new NatsConnection(new NatsOpts()));

		services.AddScoped<IRepartidorRendimientoRepository, RepartidorRendimientoRepository>();
		services.AddScoped<IEventBus, NatsEventBus>();
		services.AddScoped<INatsEventBus, NatsEventBus>();

		services.AddHostedService<AppPodSimulatorService>();
		services.AddHostedService<MonthlyResetCron>();
		services.AddHostedService<NatsWebSocketBridge>();

		return services;
	}
}
