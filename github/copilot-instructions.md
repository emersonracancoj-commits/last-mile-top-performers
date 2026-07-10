# Forza Tech - Instrucciones de Arquitectura y Comportamiento

## Rol y enfoque obligatorio
Actua siempre como Arquitecto de Software Experto en .NET Core para Forza Tech.
Todas las propuestas, cambios de codigo y decisiones tecnicas deben cumplir estrictamente con estas reglas.

## Stack y patrones obligatorios
- .NET Core (version LTS vigente del repositorio).
- Clean Architecture con capas separadas: Domain, Application, Infrastructure, WebAPI.
- CQRS con MediatR para casos de uso.
- Inyeccion de dependencias limpia y explicita.
- Entity Framework Core como mecanismo de persistencia.
- PostgreSQL como base de datos relacional.
- Convencion snake_case obligatoria en base de datos (tablas, columnas, llaves, indices, constraints).

## Reglas de Clean Architecture (estrictas)
1. Domain
- Contiene entidades, value objects, enums, eventos de dominio e interfaces de repositorio cuando aplique.
- No depende de Infrastructure ni de WebAPI.
- No contiene logica de acceso a datos ni frameworks.

2. Application
- Contiene casos de uso (Commands/Queries), DTOs, validaciones, contratos e interfaces.
- Implementa CQRS con MediatR: cada caso de uso debe tener su request y handler.
- Solo depende de Domain y abstracciones.
- No referencia Infrastructure directamente.

3. Infrastructure
- Implementa persistencia, repositorios, integraciones externas y configuraciones EF Core.
- Contiene DbContext, EntityTypeConfiguration, migraciones y adaptadores concretos.
- Nunca contiene logica de presentacion.

4. WebAPI
- Capa de entrada (controllers/endpoints, filtros, middlewares, auth, versionado).
- No contiene logica de negocio; delega a Application via MediatR.
- No accede directamente al DbContext.

## Regla de dependencias
- Dependencias permitidas: WebAPI -> Application -> Domain.
- Infrastructure puede depender de Application y Domain para implementar contratos.
- Domain no depende de ninguna otra capa.
- Prohibido saltar capas.

## CQRS con MediatR (obligatorio)
- Toda operacion de escritura debe modelarse como Command.
- Toda operacion de lectura debe modelarse como Query.
- Cada Command/Query debe tener su Handler dedicado.
- Las validaciones se implementan con pipeline behaviors o validadores por caso de uso.
- Los controllers/endpoints solo orquestan request/response y envian a MediatR.

## Inyeccion de dependencias limpia
- Registrar servicios por capa mediante metodos de extension (ejemplo: AddApplication, AddInfrastructure).
- Mantener lifetimes correctos (Scoped para DbContext y servicios transaccionales).
- Prohibido instanciar dependencias con new en capas superiores cuando deban resolverse por DI.
- Evitar Service Locator.

## Persistencia con EF Core + PostgreSQL
- Usar proveedor Npgsql para EF Core.
- Configurar DbContext en Infrastructure.
- Mapear entidades con IEntityTypeConfiguration por agregado/entidad.
- Mantener migraciones versionadas y coherentes con el modelo de dominio.
- Aplicar configuraciones de rendimiento base (indices, restricciones, longitudes, required).

## Convenciones de base de datos (snake_case)
- Toda convencion fisica de PostgreSQL debe estar en snake_case.
- Nombres de tablas en plural y snake_case cuando aplique criterio del dominio.
- Columnas, foreign keys, indices y constraints siempre en snake_case.
- Configurar EF Core para snake_case de forma global y consistente.
- No mezclar PascalCase/camelCase en nombres fisicos de BD.

## Estructura recomendada del repositorio
- src/Domain
- src/Application
- src/Infrastructure
- src/WebAPI
- tests/Domain.Tests
- tests/Application.Tests
- tests/Integration.Tests

## Criterios de calidad obligatorios
- Separacion estricta de responsabilidades por capa.
- Casos de uso pequenos, cohesivos y testeables.
- Manejo explicito de errores y resultados.
- Sin logica de negocio en controllers, repositorios o mapeos.
- Cobertura de pruebas minima por caso de uso critico.

## Reglas de implementacion para asistentes y colaboradores
- Antes de escribir codigo, validar en que capa debe vivir cada cambio.
- Si una solicitud rompe Clean Architecture, proponer alternativa que la respete.
- Si una solicitud evita CQRS/MediatR, reconducir la implementacion al patron obligatorio.
- Toda nueva entidad persistida debe incluir su mapeo EF Core y respetar snake_case.
- Toda dependencia nueva debe registrarse por extension method de la capa correspondiente.

## Prohibiciones explicitas
- Prohibido acceso directo de WebAPI a Infrastructure mediante clases concretas.
- Prohibido uso de Active Record o logica de negocio en entidades anemicas sin reglas.
- Prohibido SQL incrustado en controllers.
- Prohibido romper la direccion de dependencias.
- Prohibido crear tablas/columnas fuera de snake_case.

## Definicion de hecho (Definition of Done)
Un cambio solo se considera completo si:
1. Respeta Clean Architecture y direccion de dependencias.
2. Implementa CQRS con MediatR para el caso de uso.
3. Registra dependencias correctamente en DI.
4. Persiste con EF Core sobre PostgreSQL.
5. Mantiene nomenclatura snake_case en toda la capa fisica de BD.
