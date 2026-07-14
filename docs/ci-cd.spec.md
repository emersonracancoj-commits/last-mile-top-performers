# Especificacion de CI/CD - Forza Last-Mile Top Performers

## 1. Objetivo
Definir una estrategia CI/CD para el repositorio que garantice calidad continua, trazabilidad de versiones y despliegue seguro a Kubernetes, alineada con Clean Architecture y el stack del proyecto (.NET + Angular + PostgreSQL + NATS + Keycloak).

## 2. Alcance
Aplica a:
- Backend .NET (Domain, Application, Infrastructure, WebAPI)
- Frontend Angular
- Pruebas unitarias en backend y frontend
- Calidad de codigo y controles de seguridad basicos
- Empaquetado de imagenes Docker
- Despliegue a clustes Kubernetes por ambiente

## 3. Triggers del Pipeline

### 3.1 Trigger en main
Disparador:
- Push directo a main
- Merge de Pull Request hacia main

Comportamiento:
- Ejecutar pipeline completo CI + CD
- Generar version semantica oficial
- Etiquetar release
- Construir y publicar imagenes Docker versionadas
- Desplegar a Produccion con estrategia controlada (rolling update)

### 3.2 Trigger en ramas de desarrollo
Disparador:
- Push en ramas develop, feature/*, bugfix/*, hotfix/*
- Pull Request hacia develop o main

Comportamiento:
- Ejecutar CI completo
- Ejecutar CD solo a ambientes no productivos (Development/Staging) segun politica
- No publicar release estable en latest de produccion
- Generar etiquetas pre-release (ejemplo: 1.4.0-rc.3, 1.4.0-beta.2)

### 3.3 Trigger manual
Disparador:
- workflow_dispatch o ejecucion manual desde la plataforma CI

Comportamiento:
- Permite re-ejecutar despliegues
- Permite promover un artifact ya validado entre ambientes

## 4. Jobs de CI

## 4.1 Job: Backend Build (.NET)
Responsabilidad:
- Restaurar paquetes
- Compilar solucion y proyectos de backend

Comandos sugeridos:
- dotnet restore Forza.LastMile.TopPerformers.sln
- dotnet build Forza.LastMile.TopPerformers.sln --configuration Release --no-restore

Criterio de exito:
- Sin errores de compilacion
- Warnings criticos tratados segun politica

## 4.2 Job: Frontend Build (Angular)
Responsabilidad:
- Instalar dependencias
- Compilar frontend en modo produccion

Comandos sugeridos:
- npm ci (en src/frontend)
- npm run build

Criterio de exito:
- Build completado
- Bundle generado sin errores de TypeScript

## 4.3 Job: Unit Tests
Responsabilidad:
- Ejecutar pruebas unitarias backend y frontend

Comandos sugeridos:
- dotnet test tests --configuration Release --no-build --collect:"XPlat Code Coverage"
- npm run test -- --watch=false --browsers=ChromeHeadless

Criterio de exito:
- 100% de suites verdes
- Umbral minimo de cobertura por capa (recomendado):
  - Application >= 80%
  - Domain >= 85%
  - Frontend >= 75%

## 4.4 Job: Calidad de codigo
Responsabilidad:
- Analisis estatico y estandarizacion
- Validacion de estilo y convenciones

Controles recomendados:
- dotnet format --verify-no-changes
- eslint/angular-eslint (frontend)
- Analisis SonarQube/SonarCloud (opcional pero recomendado)
- Verificacion de secretos en repo (gitleaks o equivalente)

Criterio de exito:
- Sin issues bloqueantes/criticos
- Sin secretos detectados

## 5. Jobs de CD

## 5.1 Job: Construccion de imagenes Docker
Responsabilidad:
- Construir imagen backend y frontend
- Publicar en registry (GHCR, ACR, ECR o Docker Hub)

Tags de imagen:
- semver completo (ejemplo: 1.7.2)
- sha corto (ejemplo: sha-a1b2c3d)
- canal (dev, staging, latest-main segun rama)

Convencion sugerida:
- forza/backend:1.7.2
- forza/backend:sha-a1b2c3d
- forza/frontend:1.7.2
- forza/frontend:sha-a1b2c3d

## 5.2 Job: Etiquetado semantico
Responsabilidad:
- Calcular version segun Conventional Commits o estrategia GitVersion
- Crear tag git y release notes automatizadas

Reglas sugeridas:
- feat: incrementa MINOR
- fix: incrementa PATCH
- BREAKING CHANGE: incrementa MAJOR

Salida esperada:
- Tag git vX.Y.Z
- Metadata de release para trazabilidad

## 5.3 Job: Despliegue a Kubernetes
Responsabilidad:
- Desplegar manifests o chart Helm por ambiente

Alternativa A (kubectl):
- kubectl apply -f k8s/base
- kubectl apply -f k8s/overlays/dev|staging|prod
- kubectl rollout status deployment/forza-backend -n forza
- kubectl rollout status deployment/forza-frontend -n forza

Alternativa B (helm):
- helm upgrade --install forza-backend ./deploy/charts/backend -n forza -f values-<env>.yaml --set image.tag=<tag>
- helm upgrade --install forza-frontend ./deploy/charts/frontend -n forza -f values-<env>.yaml --set image.tag=<tag>

Politica de despliegue por rama:
- develop/feature/* -> namespace dev
- release/* -> namespace staging
- main -> namespace prod

Controles post-despliegue:
- Health checks (/health)
- Validacion de readiness/liveness probes
- Smoke tests basicos

## 6. Estrategia de Promotion entre ambientes
- Dev: despliegue automatico desde ramas de desarrollo
- Staging: despliegue automatico desde release/* o manual desde artifact aprobado
- Produccion: despliegue solo desde main con aprobacion manual opcional

## 7. Seguridad y Gobernanza
- No exponer secretos en pipeline
- Usar variables protegidas y secretos del proveedor CI
- Escaneo de vulnerabilidades de imagen (Trivy o equivalente)
- Bloquear deploy a prod si falla CI o quality gate

## 8. Artefactos y Retencion
Guardar por pipeline:
- Binarios backend
- Dist frontend
- Reportes de pruebas y cobertura
- SBOM o reporte de vulnerabilidades (si aplica)

Retencion recomendada:
- CI artifacts: 14 a 30 dias
- Release artifacts: segun politica corporativa

## 9. Definicion de Done para Pipeline
Un cambio se considera listo para promover cuando:
- CI completo exitoso
- Tests unitarios aprobados
- Quality gate aprobado
- Imagen Docker publicada con tag trazable
- Despliegue en ambiente objetivo validado
