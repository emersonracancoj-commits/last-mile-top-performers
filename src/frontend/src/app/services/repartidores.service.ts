import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';

export interface RepartidorRendimientoDto {
  id: string;
  nombre_repartidor: string;
  tipo_vehiculo: string;
  satisfaccion_score: number;
  entregas_completadas: number;
  meta_diaria_original: number;
  meta_diaria_actual: number;
  is_meta_ajustada: boolean;
  incidencia_descripcion: string | null;
  mes_periodo: string;
  ultima_actualizacion: string;
}

@Injectable({
  providedIn: 'root',
})
export class RepartidoresService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/repartidores';

  getRankingActual(): Observable<RepartidorRendimientoDto[]> {
    return this.http
      .get<unknown[]>(this.baseUrl)
      .pipe(map((response) => (response ?? []).map((item) => this.normalizeDto(item))));
  }

  ajustarMeta(id: string, descripcion: string, meta: number): Observable<void> {
    return this.http.patch<void>(`${this.baseUrl}/${id}/meta`, {
      incidenciaDescripcion: descripcion,
      nuevaMetaSugerida: meta,
    });
  }

  private normalizeDto(item: unknown): RepartidorRendimientoDto {
    const source = (item as Record<string, unknown>) ?? {};

    return {
      id: String(source['id'] ?? source['Id'] ?? ''),
      nombre_repartidor: String(source['nombre_repartidor'] ?? source['nombreRepartidor'] ?? source['NombreRepartidor'] ?? ''),
      tipo_vehiculo: String(source['tipo_vehiculo'] ?? source['tipoVehiculo'] ?? source['TipoVehiculo'] ?? ''),
      satisfaccion_score: Number(source['satisfaccion_score'] ?? source['satisfaccionScore'] ?? source['SatisfaccionScore'] ?? 0),
      entregas_completadas: Number(source['entregas_completadas'] ?? source['entregasCompletadas'] ?? source['EntregasCompletadas'] ?? 0),
      meta_diaria_original: Number(source['meta_diaria_original'] ?? source['metaDiariaOriginal'] ?? source['MetaDiariaOriginal'] ?? 0),
      meta_diaria_actual: Number(source['meta_diaria_actual'] ?? source['metaDiariaActual'] ?? source['MetaDiariaActual'] ?? 0),
      is_meta_ajustada: Boolean(source['is_meta_ajustada'] ?? source['isMetaAjustada'] ?? source['IsMetaAjustada'] ?? false),
      incidencia_descripcion: (source['incidencia_descripcion'] ?? source['incidenciaDescripcion'] ?? source['IncidenciaDescripcion'] ?? null) as string | null,
      mes_periodo: String(source['mes_periodo'] ?? source['mesPeriodo'] ?? source['MesPeriodo'] ?? ''),
      ultima_actualizacion: String(source['ultima_actualizacion'] ?? source['ultimaActualizacion'] ?? source['UltimaActualizacion'] ?? ''),
    };
  }
}
