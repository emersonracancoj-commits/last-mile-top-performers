# Forza Kubernetes deployment script (PowerShell optimized)
$NAMESPACE = "forza"

Write-Host "[1/4] Ensuring namespace '$NAMESPACE' exists..." -ForegroundColor Cyan
if (-not (kubectl get namespace $NAMESPACE -ErrorAction SilentlyContinue)) {
    kubectl create namespace $NAMESPACE
}

Write-Host "[2/4] Applying ConfigMap..." -ForegroundColor Cyan
kubectl apply -n $NAMESPACE -f k8s/04-configmap.yaml

Write-Host "[3/4] Applying Deployments..." -ForegroundColor Cyan
kubectl apply -n $NAMESPACE -f k8s/01-api-deployment.yaml
kubectl apply -n $NAMESPACE -f k8s/02-frontend-deployment.yaml

Write-Host "[4/4] Applying Services..." -ForegroundColor Cyan
kubectl apply -n $NAMESPACE -f k8s/03-services.yaml

Write-Host "Current pod status in namespace '$NAMESPACE':" -ForegroundColor Green
kubectl get pods -n $NAMESPACE