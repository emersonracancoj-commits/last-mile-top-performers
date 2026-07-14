# Forza Kubernetes deployment script (PowerShell optimized)
$NAMESPACE = "forza"
$IMAGE_PULL_SECRET_NAME = "ghcr-pull-secret"

Write-Host "[1/6] Ensuring namespace '$NAMESPACE' exists..." -ForegroundColor Cyan
if (-not (kubectl get namespace $NAMESPACE -ErrorAction SilentlyContinue)) {
    kubectl create namespace $NAMESPACE
}

Write-Host "[2/6] Applying ConfigMap..." -ForegroundColor Cyan
kubectl apply -n $NAMESPACE -f k8s/04-configmap.yaml

Write-Host "[3/6] Applying image pull secret..." -ForegroundColor Cyan
kubectl apply -n $NAMESPACE -f k8s/05-image-pull-secret.yaml

Write-Host "[4/6] Linking image pull secret to serviceaccounts..." -ForegroundColor Cyan
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

Write-Host "Current pod status in namespace '$NAMESPACE':" -ForegroundColor Green
kubectl get pods -n $NAMESPACE