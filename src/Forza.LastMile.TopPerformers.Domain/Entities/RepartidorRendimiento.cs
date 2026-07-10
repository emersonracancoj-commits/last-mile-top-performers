using System.ComponentModel.DataAnnotations.Schema;

namespace Forza.LastMile.TopPerformers.Domain.Entities;

public class RepartidorRendimiento
{
	[Column("id")]
	public Guid Id { get; private set; }
	[Column("nombre_repartidor")]
	public string NombreRepartidor { get; private set; }
	[Column("tipo_vehiculo")]
	public string TipoVehiculo { get; private set; }
	[Column("satisfaccion_score")]
	public decimal SatisfaccionScore { get; private set; }
	[Column("entregas_completadas")]
	public int EntregasCompletadas { get; private set; }
	[Column("meta_diaria_original")]
	public int MetaDiariaOriginal { get; private set; }
	[Column("meta_diaria_actual")]
	public int MetaDiariaActual { get; private set; }
	[Column("is_meta_ajustada")]
	public bool IsMetaAjustada { get; private set; }
	[Column("incidencia_descripcion")]
	public string? IncidenciaDescripcion { get; private set; }
	[Column("mes_periodo")]
	public string MesPeriodo { get; private set; }
	[Column("ultima_actualizacion")]
	public DateTime UltimaActualizacion { get; private set; }

	public RepartidorRendimiento(
		Guid id,
		string nombreRepartidor,
		string tipoVehiculo,
		decimal satisfaccionScore,
		int entregasCompletadas,
		int metaDiariaOriginal,
		int metaDiariaActual,
		bool isMetaAjustada,
		string? incidenciaDescripcion,
		string mesPeriodo,
		DateTime ultimaActualizacion)
	{
		Id = id;
		NombreRepartidor = nombreRepartidor;
		TipoVehiculo = tipoVehiculo;
		SatisfaccionScore = satisfaccionScore;
		EntregasCompletadas = entregasCompletadas;
		MetaDiariaOriginal = metaDiariaOriginal;
		MetaDiariaActual = metaDiariaActual;
		IsMetaAjustada = isMetaAjustada;
		IncidenciaDescripcion = incidenciaDescripcion;
		MesPeriodo = mesPeriodo;
		UltimaActualizacion = ultimaActualizacion;
	}

	public void AjustarMetaPorIncidencia(string descripcion, int nuevaMeta)
	{
		MetaDiariaActual = nuevaMeta;
		IsMetaAjustada = true;
		IncidenciaDescripcion = descripcion;
	}

	public void IncrementarEntregasCompletadas()
	{
		EntregasCompletadas++;
		UltimaActualizacion = DateTime.UtcNow;
	}

	public void ResetearParaNuevoMes(string mesPeriodo)
	{
		EntregasCompletadas = 0;
		MesPeriodo = mesPeriodo;
		UltimaActualizacion = DateTime.UtcNow;
	}
}
