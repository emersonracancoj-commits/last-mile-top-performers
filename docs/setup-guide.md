# Setup Guide - Forza Last-Mile Top Performers

## Objetivo
Automatizar el ciclo local y CI/CD para despliegue en Kubernetes sin pasos manuales repetitivos.

## Prerrequisitos locales
- Docker instalado y corriendo
- kind instalado
- kubectl instalado
- PowerShell 7+ o Windows PowerShell

## Script de autoconfiguracion
Archivo:
- setup-env.ps1

Que hace:
1. Verifica Docker, kind y kubectl.
2. Crea (o reutiliza) el cluster kind con nombre forza-cluster.
3. Crea (o reutiliza) namespace forza.
4. Aplica manifiestos en orden:
   - ConfigMap
   - Secret/imagePullSecret
   - Deployments
   - Services
5. Configura imagePullSecret para GHCR y lo vincula a serviceaccounts.
6. Carga imagenes locales (si existen) forza-api:local y forza-frontend:local en kind.
7. Muestra estado final de pods y services.

## Como ejecutar
Desde la raiz del repositorio:

PowerShell:
- ./setup-env.ps1

Si vas a crear el secret de GHCR automaticamente, define variables antes de ejecutar:

PowerShell:
- $env:GHCR_USERNAME = "<tu_usuario_ghcr>"
- $env:GHCR_TOKEN = "<tu_token_ghcr>"
- $env:GHCR_EMAIL = "<tu_email>"  (opcional)
- ./setup-env.ps1

## Secretos de GitHub (configurar una sola vez)
En GitHub > Settings > Secrets and variables > Actions:

Obligatorios para CI/CD:
- GHCR_USERNAME
- GHCR_TOKEN
- KUBE_CONFIG_DATA

Opcional:
- GHCR_EMAIL

## Notas de KUBE_CONFIG_DATA
- Puede almacenarse en base64 o texto plano del kubeconfig.
- El pipeline detecta ambos formatos.

## Flujo CI/CD automatizado
El workflow en .github/workflows/ci-cd.yml hace lo siguiente en push a main:
1. Build y push de imagen API a GHCR.
2. Build y push de imagen Frontend a GHCR.
3. Configura kubectl con KUBE_CONFIG_DATA.
4. Ejecuta deploy.ps1.
5. Actualiza Deployments con tags por commit SHA.
6. Verifica rollout en namespace forza.

## Solucion de problemas
- Error de autenticacion GHCR: verifica GHCR_USERNAME/GHCR_TOKEN y permisos read:packages/write:packages.
- ImagePullBackOff: valida que el imagePullSecret este creado y vinculado al serviceaccount del namespace forza.
- Cluster no encontrado: ejecuta de nuevo setup-env.ps1 para recrear/reusar kind-forza-cluster.
