# Forza Kubernetes deployment script (PowerShell optimized)
# Uso: .\deploy.ps1
$NAMESPACE = "forza"
$IMAGE_PULL_SECRET_NAME = "ghcr-pull-secret"

# Función para manejar el secreto de registro de Docker (GHCR)
function New-OrUpdateGhcrSecret {
    param(
        [string]$Namespace,
        [string]$SecretName
    )

    if ($env:GHCR_USERNAME -and $env:GHCR_TOKEN) {
        Write-Host "Creating/Updating GHCR image pull secret from environment variables..." -ForegroundColor Yellow
        
        # Eliminamos si existe para asegurar la actualización
        kubectl delete secret $SecretName -n $Namespace --ignore-not-found
        
        # Creamos el secreto de registro docker
        kubectl create secret docker-registry $SecretName `
            --namespace $Namespace `
            --docker-server=ghcr.io `
            --docker-username=$env:GHCR_USERNAME `
            --docker-password=$env:GHCR_TOKEN
    }
    else {
        Write-Host "GHCR_USERNAME/TOKEN not found. Applying fallback k8s/05-image-pull-secret.yaml..." -ForegroundColor Yellow
        kubectl apply -n $Namespace -f k8s/05-image-pull-secret.yaml
    }
}

Write-Host "[1/6] Ensuring namespace '$NAMESPACE' exists..." -ForegroundColor Cyan
if (-not (kubectl get namespace $NAMESPACE -ErrorAction SilentlyContinue)) {
    kubectl create namespace $NAMESPACE
}

Write-Host "[2/6] Applying ConfigMap..." -ForegroundColor Cyan
kubectl apply -n $NAMESPACE -f k8s/04-configmap.yaml

Write-Host "[3/6] Applying image pull secret..." -ForegroundColor Cyan
New-OrUpdateGhcrSecret -Namespace $NAMESPACE -SecretName $IMAGE_PULL_SECRET_NAME

Write-Host "[4/6] Linking image pull secret to all ServiceAccounts..." -ForegroundColor Cyan
# Vinculamos a todas las cuentas de servicio para asegurar que cualquier Pod pueda bajar la imagen
$serviceAccounts = kubectl get serviceaccount -n $NAMESPACE -o jsonpath="{.items[*].metadata.name}"
if ($serviceAccounts) {
    $serviceAccounts.Split(" ") | Where-Object { $_ -and $_.Trim() -ne "" } | ForEach-Object {
        kubectl patch serviceaccount $_ -n $NAMESPACE -p "{\"imagePullSecrets\":[{\"name\":\"$IMAGE_PULL_SECRET_NAME\"}]}"
    }
}

Write-Host "[5/6] Applying Deployments..." -ForegroundColor Cyan
kubectl apply -n $NAMESPACE -f k8s/01-api-deployment.yaml
kubectl apply -n $NAMESPACE -f k8s/02-frontend-deployment.yaml

Write-Host "[6/6] Applying Services..." -ForegroundColor Cyan
kubectl apply -n $NAMESPACE -f k8s/03-services.yaml

Write-Host "Deployment completed. Current pod status:" -ForegroundColor Green
kubectl get pods -n $NAMESPACE