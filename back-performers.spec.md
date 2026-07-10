# Spec del Backend: Forza Last-Mile Top Performers

## Stack Tecnológico
- API: .NET Core 8 Web API con Clean Architecture
- Patrón: CQRS utilizando MediatR
- Base de Datos: PostgreSQL mediante Entity Framework Core
- Mensajería: NATS JetStream para publicación de eventos
- Autenticación: Sola lectura de JWT emitido por Keycloak SSO

## Entidad Base (PostgreSQL - snake_case)
Tabla: `repartidor_rendimiento`
- id (Guid, PK)
- nombre_repartidor (string)
- tipo_vehiculo (string: Motocicleta, Panel, Bicicleta Eléctrica)
- satisfaccion_score (decimal, 1.0 a 5.0)
- entregas_completadas (int)
- meta_diaria_original (int)
- meta_diaria_actual (int)
- is_meta_ajustada (boolean)
- incidencia_descripcion (string, nullable)
- mes_periodo (string, formato "YYYY-MM")
- ultima_actualizacion (DateTime)

## Casos de Uso (CQRS)
1. Query: `GetTopPerformersQuery` -> Trae la lista de repartidores ordenados por entregas_completadas (descendente) del mes_periodo actual.
2. Command: `AjustarMetaPorIncidenciaCommand` -> Recibe id del repartidor y el texto de la incidencia. Simula la IA reduciendo la meta diaria y activando el flag is_meta_ajustada.

## Eventos de Dominio (NATS)
1. `delivery.repartidor_rendimiento.transaccion_recibida`
2. `delivery.repartidor_rendimiento.meta_ajustada`

## Servicios en Segundo Plano (Hosted Services)
- `AppPodSimulatorService`: Un BackgroundService de .NET que cada 4 segundos elija un repartidor al azar e incremente su propiedad `entregas_completadas` simulando el POD de la calle, guarde en BD y dispare el evento de NATS correspondiente.
- `MonthlyResetCron`: Simulación lógica para limpiar/reiniciar contadores al cambiar de mes.