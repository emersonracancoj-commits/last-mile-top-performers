using Forza.LastMile.TopPerformers.Application;
using Forza.LastMile.TopPerformers.Application.Common.Interfaces;
using Forza.LastMile.TopPerformers.Infrastructure;
using Forza.LastMile.TopPerformers.WebAPI.Hubs;
using Forza.LastMile.TopPerformers.WebAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

builder.Services.AddApplication();
builder.Services.AddInfrastructure(connectionString);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddCors(options => {
    options.AddPolicy("AllowAngular", policy => {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Registrar SignalR para notificaciones en tiempo real
builder.Services.AddSignalR();

// Registrar implementación de WebSocket notification publisher
builder.Services.AddSingleton<IWebSocketNotificationPublisher, NotificationPublisher>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseCors("AllowAngular");

app.MapControllers();

// Mapear el SignalR Hub
app.MapHub<NotificationHub>("/hubs/notification");

app.Run();
