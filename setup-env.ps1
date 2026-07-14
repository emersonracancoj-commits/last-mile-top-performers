$ErrorActionPreference = "Stop"

$ClusterName = "forza-cluster"
$Namespace = "forza"
$ImagePullSecretName = "ghcr-pull-secret"

function Assert-Command {
    param([string]$Name)

    if (-not (Get-Command $Name -ErrorAction SilentlyContinue)) {
        throw "Required command '$Name' is not installed or not in PATH."
    }
}

function Ensure-KindCluster {
    param([string]$Name)

    $clusters = kind get clusters 2>$null
    if ($clusters -notcontains $Name) {
        Write-Host "Creating kind cluster '$Name'..." -ForegroundColor Cyan
        kind create cluster --name $Name
    }
    else {
        Write-Host "Kind cluster '$Name' already exists." -ForegroundColor Yellow
    }

    kubectl config use-context "kind-$Name" | Out-Null
}

function Ensure-Namespace {
    param([string]$Name)

    if (-not (kubectl get namespace $Name -ErrorAction SilentlyContinue)) {
        kubectl create namespace $Name | Out-Null
        Write-Host "Namespace '$Name' created." -ForegroundColor Green
    }
    else {
        Write-Host "Namespace '$Name' already exists." -ForegroundColor Yellow
    }
}

function Ensure-GhcrPullSecret {
    param(
        [string]$NamespaceName,
        [string]$SecretName
    )

    if ($env:GHCR_USERNAME -and $env:GHCR_TOKEN) {
        $email = if ($env:GHCR_EMAIL) { $env:GHCR_EMAIL } else { "devops@forza.local" }

        Write-Host "Creating/updating GHCR imagePullSecret from environment variables..." -ForegroundColor Cyan
        kubectl create secret docker-registry $SecretName `
            --namespace $NamespaceName `
            --docker-server=ghcr.io `
            --docker-username=$env:GHCR_USERNAME `
            --docker-password=$env:GHCR_TOKEN `
            --docker-email=$email `
            --dry-run=client -o yaml | kubectl apply -f - | Out-Null
    }
    else {
        Write-Host "GHCR credentials are not set in env. Applying manifest fallback k8s/05-image-pull-secret.yaml." -ForegroundColor Yellow
        kubectl apply -n $NamespaceName -f k8s/05-image-pull-secret.yaml | Out-Null
    }

    $serviceAccounts = kubectl get serviceaccount -n $NamespaceName -o jsonpath="{.items[*].metadata.name}"
    if ($serviceAccounts) {
        $serviceAccounts.Split(" ") | Where-Object { $_ -and $_.Trim() -ne "" } | ForEach-Object {
            kubectl patch serviceaccount $_ -n $NamespaceName -p "{\"imagePullSecrets\":[{\"name\":\"$SecretName\"}]}" | Out-Null
        }
        Write-Host "imagePullSecret linked to serviceaccounts in namespace '$NamespaceName'." -ForegroundColor Green
    }
}

function Load-LocalImagesToKind {
    param([string]$KindClusterName)

    $images = @("forza-api:local", "forza-frontend:local")
    foreach ($image in $images) {
        docker image inspect $image *> $null
        if ($LASTEXITCODE -eq 0) {
            Write-Host "Loading image '$image' into kind cluster '$KindClusterName'..." -ForegroundColor Cyan
            kind load docker-image $image --name $KindClusterName | Out-Null
        }
        else {
            Write-Host "Image '$image' not found locally. Skipping kind image load for this image." -ForegroundColor Yellow
        }
    }
}

Write-Host "Validating required tools..." -ForegroundColor Cyan
Assert-Command -Name docker
Assert-Command -Name kind
Assert-Command -Name kubectl

Ensure-KindCluster -Name $ClusterName
Ensure-Namespace -Name $Namespace

Write-Host "Applying manifests in strict order (Namespace -> ConfigMap -> Secrets -> Deployments -> Services)..." -ForegroundColor Cyan
kubectl apply -n $Namespace -f k8s/04-configmap.yaml | Out-Null
kubectl apply -n $Namespace -f k8s/05-image-pull-secret.yaml | Out-Null
Ensure-GhcrPullSecret -NamespaceName $Namespace -SecretName $ImagePullSecretName
kubectl apply -n $Namespace -f k8s/01-api-deployment.yaml | Out-Null
kubectl apply -n $Namespace -f k8s/02-frontend-deployment.yaml | Out-Null
kubectl apply -n $Namespace -f k8s/03-services.yaml | Out-Null

Load-LocalImagesToKind -KindClusterName $ClusterName

Write-Host "Deployment summary:" -ForegroundColor Green
kubectl get pods -n $Namespace
kubectl get svc -n $Namespace
