# Propuesta de Proyecto Capstone: Forza Last-Mile Top Performers

## 1. Descripción del Problema
En el área de operaciones de Delivery de última milla de Forza, el seguimiento del rendimiento diario de los repartidores y la satisfacción final del cliente son métricas críticas para asegurar la eficiencia del servicio y el pago justo de bonificaciones. Actualmente, los repartidores registran el cierre de sus entregas en la aplicación POD (*Proof of Delivery*) en la calle, recolectando datos como firmas, fotos y nombres de clientes.

El problema operativo surge ante los **imprevistos inevitables en ruta** (ej. accidentes, llantas pinchadas, inundaciones por clima o bloqueos severos de tráfico). Cuando ocurren estas incidencias, el flujo logístico tradicional penaliza injustamente la meta diaria del repartidor, lo que puede provocar la pérdida de su bono por causas ajenas a su desempeño. Actualmente, no existe una plataforma visual centralizada que consolide el ranking de rendimiento reactivo en tiempo real y que, al mismo tiempo, permita a la supervisión gestionar de forma ágil y asistida la mitigación o flexibilización de metas por imprevistos mediante un criterio estandarizado.

## 2. Descripción de la Solución (Caso de Uso)
**Forza Last-Mile Top Performers** es un dashboard administrativo interno de una sola página destinado al Supervisor de Delivery. El sistema centraliza la visualización de la flota y automatiza el ajuste justo de objetivos diarios.

El flujo unificado de la aplicación consiste en:
1. El supervisor de Forza inicia sesión de manera segura utilizando las credenciales corporativas centralizadas en **Keycloak SSO**.
2. Al ingresar, visualiza un *Leaderboard* en tiempo real con un diseño de rejilla responsiva (*Grid*) estilizado con Tailwind CSS. Las tarjetas destacan con un formato de podio (puestos 1, 2 y 3) a los repartidores con mayor volumen de entregas, mostrando su avatar, tipo de vehículo, score de satisfacción promedio (estrellas) y una barra de progreso de entregas (`entregas_completadas / meta_diaria`).
3. El backend en **.NET Core** cuenta con un servicio secundario automático que emula de fondo las transacciones de la app móvil POD en calle, inyectando actualizaciones constantes que reordenan las posiciones del podio instantáneamente mediante *Angular Signals* y mensajería **NATS JetStream**.
4. Si un repartidor sufre un percance, el supervisor hace clic sobre su tarjeta y abre un **modal flotante de incidencias**. El supervisor escribe el imprevisto en lenguaje natural (ej. *"Repartidor reporta falla mecánica de embrague en ruta de la Zona 18 y espera grúa"*).
5. La Inteligencia Artificial (IA) integrada en el backend procesa el texto, evalúa la gravedad y responde en el mismo modal sugiriendo una reducción justa (ej. *"Gravedad: Alta. Meta sugerida para el bono: Reducir de 20 a 14 paquetes"*).
6. Al presionar "Aprobar Ajuste", los datos se guardan en **PostgreSQL**, la barra de progreso se recalcula y la tarjeta del repartidor en el grid principal muestra de forma permanente un **badge reflectivo indicador** (ej. un icono de balanza o una etiqueta de *"Meta Ajustada por IA"*) para mantener la transparencia operativa. El pool de métricas e histórico se reinicia automáticamente al iniciar cada mes calendario.

## 3. Delimitación del Alcance (Scope)
Para garantizar la viabilidad técnica y cumplir el límite estricto de desarrollo de la academia (10 a 15 horas totales), el alcance se acota estrictamente:

### In-Scope (Lo que sí incluye)
* **Seguridad:** Autenticación e interceptores HTTP con Keycloak SSO.
* **Frontend (Angular 17+ y Tailwind CSS):**
  * **Pantalla 1 (Grid Layout de Podio):** Interfaz reactiva donde las tarjetas cambian de posición de forma animada basándose en *Angular Signals* y escuchando los eventos del bus de mensajería. Incluye Badges de estado de metas y el indicador visual de ajuste por IA.
  * **Pantalla 2 (Modal de Incidencias):** Ventana emergente con entrada de texto libre para invocar la API del motor de IA simulado, botones de Tailwind y visualización del veredicto sugerido.
* **Backend (.NET Core):** API construida bajo Clean Architecture utilizando CQRS con MediatR. Incluye un *hosted service* en segundo plano para inyectar transacciones automatizadas aleatorias simulando la app POD, un servicio de respuestas mockeadas de IA según palabras clave, y un cron/bucle mensual para purgar/reiniciar contadores.
* **Base de Datos:** Persistencia del estado diario de la flota en tablas PostgreSQL.
* **Arquitectura Orientada a Eventos:** Publicación de tópicos en NATS JetStream para la actualización en vivo.

### Out-of-Scope (Lo que no incluye)
* Construcción, despliegue o alteración de la aplicación móvil POD real utilizada por los repartidores en calle.
* Integración con APIs de mapas en tiempo real (Google Maps / Mapbox) o tracking geográfico por GPS.
* Roles avanzados de analítica, descarga de reportes en PDF, ni alertas externas mediante SMS o correos electrónicos reales.

## 4. Estructura de Datos (Entidad Principal)
Los datos persistirán en PostgreSQL implementando nombres de tablas y campos utilizando el formato corporativo `snake_case`.

### Entidad: `repartidor_rendimiento`
* `id` (UUID, Primary Key)
* `nombre_repartidor` (VARCHAR)
* `tipo_vehiculo` (VARCHAR, ej: "Panel", "Automóvil")
* `satisfaccion_score` (NUMERIC, formato decimal de calificación de clientes de 1.0 a 5.0)
* `entregas_completadas` (INTEGER, incrementado automáticamente por el simulador de fondo)
* `meta_diaria_original` (INTEGER, meta base por defecto)
* `meta_diaria_actual` (INTEGER, meta vigente tras la evaluación del imprevisto)
* `is_meta_ajustada` (BOOLEAN, flag que activa el badge visual en el grid de tarjetas)
* `incidencia_descripcion` (TEXT, bitácora en lenguaje natural procesada por la IA)
* `mes_periodo` (VARCHAR, formato "YYYY-MM" para control de reinicios mensuales)
* `ultima_actualizacion` (TIMESTAMP)

## 5. Eventos de Dominio
Siguiendo la arquitectura dirigida por eventos (EDD) y la nomenclatura `dominio.entidad.accion` obligatoria de Forza:

1. `delivery.repartidor_rendimiento.transaccion_recibida`: Se dispara de forma asíncrona cada vez que el simulador de fondo emula una entrega exitosa desde la calle. *Efecto secundario:* Incrementa el contador de la base de datos y empuja el cambio reactivo hacia el podio en Angular.
2. `delivery.repartidor_rendimiento.meta_ajustada`: Se dispara de manera inmediata cuando el supervisor aprueba la sugerencia del modal de IA. *Efecto secundario:* Modifica la propiedad `meta_diaria_actual`, activa `is_meta_ajustada` a `true` y publica el mensaje en NATS para pintar el badge de alerta en el Frontend.

## 6. Arquitectura del Sistema (Diagrama C4 Nivel 1)
Documentación de contexto del Capstone generada en la sintaxis estándar de Mermaid:

```mermaid
flowchart TD
    Supervisor([Supervisor de Delivery]) -->|Monitorea y gestiona incidencias| FE[[Dashboard Frontend: Angular + Tailwind]]
    FE -->|Endpoints seguros HTTP REST| GW[API Gateway Corporativo: APISIX]
    GW -->|Enruta peticiones protegidas con JWT| BE[[Backend API: .NET Core + Clean Architecture]]
    BE -->|Valida tokens y firmas de acceso| KC[(Keycloak SSO)]
    BE -->|Almacena rendimiento y auditorías| DB[(PostgreSQL)]
    BE -->|Transmite telemetría y ajustes en vivo| NS[(NATS JetStream)]
    BE -.->|Simulación: Evalúa gravedad del texto de ruta| AI[Engine IA del Backend]

    style FE fill:#dee2e6,stroke:#343a40,stroke-width:2px
    style BE fill:#dee2e6,stroke:#343a40,stroke-width:2px
    style DB fill:#ced4da,stroke:#495057,stroke-width:1px
    style KC fill:#ced4da,stroke:#495057,stroke-width:1px
    style NS fill:#ced4da,stroke:#495057,stroke-width:1px
    style AI fill:#e9ecef,stroke:#6c757d,stroke-width:1px,stroke-dasharray: 5 5