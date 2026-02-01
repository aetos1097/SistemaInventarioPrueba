# Script de despliegue - ProductsSales IaC (Bicep)
# Uso: .\deploy.ps1 -SqlPassword "TuPassword123!" -JwtSecret "TuJwtSecret32Chars..."

param(
    [Parameter(Mandatory = $true)]
    [string]$SqlPassword,
    
    [Parameter(Mandatory = $false)]
    [string]$JwtSecret = "ProductsSales_SecretKey_Minimum_32_Characters_Long_For_HS256",
    
    [Parameter(Mandatory = $false)]
    [string]$Location = "eastus2",
    
    [Parameter(Mandatory = $false)]
    [ValidateSet("dev", "staging", "prod")]
    [string]$Environment = "dev",
    
    [Parameter(Mandatory = $false)]
    [switch]$WhatIf,
    
    [Parameter(Mandatory = $false)]
    [switch]$SqlOnly  # Solo despliega SQL (sin App Service). Usar cuando no hay cuota.
)

$ErrorActionPreference = "Stop"
$PSNativeCommandUseErrorActionPreference = $false

Write-Host "=== ProductsSales - Despliegue Bicep ===" -ForegroundColor Cyan
Write-Host "Ubicacion: $Location" -ForegroundColor Gray
Write-Host "Ambiente: $Environment" -ForegroundColor Gray
if ($SqlOnly) {
    Write-Host "Modo: Solo SQL (sin App Service)" -ForegroundColor Yellow
}
Write-Host ""

# Validar que Azure CLI esta instalado
if (-not (Get-Command az -ErrorAction SilentlyContinue)) {
    Write-Error "Azure CLI no encontrado. Instala desde: https://aka.ms/installazurecliwindows"
}

# Verificar login
$account = az account show 2>$null | ConvertFrom-Json
if (-not $account) {
    Write-Host "Iniciando sesion en Azure..." -ForegroundColor Yellow
    az login
}

# Validar plantilla Bicep
Write-Host "Validando plantilla Bicep..." -ForegroundColor Yellow
az bicep build --file main.bicep --only-show-errors
if ($LASTEXITCODE -ne 0) {
    Write-Error "Error al validar la plantilla Bicep"
}

Write-Host "Plantilla validada correctamente." -ForegroundColor Green
Write-Host ""

# Bicep espera "true"/"false" en minúsculas; PowerShell convierte $false a "False"
$deployAppService = -not $SqlOnly
$deployAppServiceBicep = $deployAppService.ToString().ToLowerInvariant()
$deployParams = "location=$Location", "environment=$Environment", "deployAppService=$deployAppServiceBicep", "sqlAdminPassword=$SqlPassword", "jwtSecretKey=$JwtSecret"
$deploymentName = "productssales-$(Get-Date -Format 'yyyyMMdd-HHmmss')"

if ($WhatIf) {
    Write-Host "Ejecutando what-if (preview del despliegue)..." -ForegroundColor Yellow
    az deployment sub create `
        --name $deploymentName `
        --location $Location `
        --template-file main.bicep `
        --parameters $deployParams `
        --what-if
} else {
    Write-Host "Desplegando infraestructura..." -ForegroundColor Yellow
    Write-Host "Nombre del despliegue: $deploymentName" -ForegroundColor Gray
    $deploymentOutput = az deployment sub create `
        --name $deploymentName `
        --location $Location `
        --template-file main.bicep `
        --parameters $deployParams `
        --only-show-errors `
        --output json 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        $deployment = $deploymentOutput | ConvertFrom-Json
        $outputs = $deployment.properties.outputs
        Write-Host ""
        Write-Host "=== Despliegue completado ===" -ForegroundColor Green
        Write-Host "Resource Group: $($outputs.resourceGroupName.value)" -ForegroundColor Cyan
        Write-Host "SQL Server: $($outputs.sqlServerFqdn.value)" -ForegroundColor Cyan
        if ($deployAppService) {
            Write-Host "URL de la API: $($outputs.webAppUrl.value)" -ForegroundColor Cyan
            Write-Host ""
            Write-Host "Siguiente paso: Publicar la API con 'dotnet publish' y desplegar al App Service" -ForegroundColor Yellow
        } else {
            Write-Host ""
            Write-Host "Modo SQL-only. Para conectar: Server=$($outputs.sqlServerFqdn.value);Database=ProductsSalesDb;User ID=sqladmin;Password=<tu-password>;Encrypt=True;TrustServerCertificate=False;" -ForegroundColor Yellow
            Write-Host "Ejecuta la API localmente apuntando a Azure SQL, o solicita cuota para App Service y redeploy sin -SqlOnly" -ForegroundColor Gray
        }
    } else {
        Write-Host $deploymentOutput -ForegroundColor Red
        Write-Error "Error en el despliegue"
    }
}
