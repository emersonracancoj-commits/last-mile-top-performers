namespace Forza.LastMile.TopPerformers.Domain.Common.Events;

public sealed record MetaAjustadaEvent(
	Guid RepartidorId,
	int MetaOriginal,
	int MetaNueva,
	string IncidenciaDescripcion,
	DateTime Timestamp);
