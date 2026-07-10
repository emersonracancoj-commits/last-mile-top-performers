namespace Forza.LastMile.TopPerformers.Domain.Common.Events;

public sealed record TransaccionRecibidaEvent(
	Guid RepartidorId,
	int EntregasCompletadas,
	DateTime Timestamp);
