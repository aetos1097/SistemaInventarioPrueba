# ProductsSales - Infraestructura como Código (Bicep)

Este directorio contiene la definición de infraestructura en **Bicep** para desplegar ProductsSales en Azure.

## Recursos desplegados

| Recurso | Descripción |
|---------|-------------|
| **Resource Group** | Contenedor de todos los recursos |
| **Azure SQL Server + Database** | Base de datos ProductsSalesDb (SKU Basic) |
| **App Service Plan** | Plan F1 Free (Linux) — *opcional* |
| **App Service** | API de ProductsSales (.NET 8) — *opcional* |

### Modo SQL-only

Cuando la suscripción no tiene cuota para App Service (Free VMs o Basic VMs = 0), se puede desplegar solo la base de datos con el parámetro `-SqlOnly`. La API se ejecuta localmente conectándose a Azure SQL.

## Prerrequisitos

1. **Azure CLI** instalado: [Instalar Azure CLI](https://aka.ms/installazurecliwindows)
2. **Suscripción de Azure** activa
3. Permisos para crear recursos en la suscripción

### Contraseña de SQL

La contraseña del administrador de Azure SQL debe cumplir:
- Mínimo 8 caracteres
- Mayúsculas, minúsculas, números y símbolos (ej: `!`, `@`, `#`)

## Despliegue

### Opción 1: Despliegue completo (con App Service)

```powershell
cd infrastructure/bicep

# Desplegar (dev)
.\deploy.ps1 -SqlPassword "TuPasswordSeguro123!"

# Con opciones
.\deploy.ps1 -SqlPassword "TuPassword123!" -JwtSecret "MiClaveJWT32Caracteres" -Environment dev -Location eastus2

# Preview (what-if) — ver cambios sin aplicar
.\deploy.ps1 -SqlPassword "TuPassword123!" -WhatIf
```

### Opción 2: Solo base de datos (sin App Service)

Usar cuando la suscripción no tiene cuota para App Service:

```powershell
.\deploy.ps1 -SqlPassword "TuPasswordSeguro123!" -SqlOnly
```

Después del despliegue, ejecuta la API localmente con la connection string mostrada en la salida.

### Opción 3: Comando manual (sin script)

```powershell
az login
cd infrastructure/bicep

az deployment sub create `
  --name productssales-$(Get-Date -Format 'yyyyMMdd-HHmmss') `
  --location eastus2 `
  --template-file main.bicep `
  --parameters location=eastus2 environment=dev deployAppService=true `
  --parameters sqlAdminPassword='TuPassword123!' jwtSecretKey='TuJwtSecret32Chars'
```

## Parámetros del script deploy.ps1

| Parámetro | Obligatorio | Descripción |
|-----------|-------------|-------------|
| `-SqlPassword` | Sí | Contraseña del administrador de SQL |
| `-JwtSecret` | No | Clave JWT (default: ProductsSales_SecretKey_...) |
| `-Location` | No | Región Azure (default: eastus2) |
| `-Environment` | No | Ambiente: dev, staging, prod (default: dev) |
| `-WhatIf` | No | Preview del despliegue sin aplicar |
| `-SqlOnly` | No | Solo despliega SQL, sin App Service |

## Después del despliegue

### Si desplegaste con App Service (modo completo)

1. **Publicar la API**:
   ```powershell
   cd c:\ProductSales\ProductsSales
   dotnet publish ProductsSales.Api/ProductsSales.Api.csproj -c Release -o ./publish
   Compress-Archive -Path ./publish/* -DestinationPath ./publish.zip -Force
   ```

2. **Desplegar al App Service**:
   ```powershell
   az webapp deployment source config-zip `
     --resource-group rg-productssales-dev `
     --name api-productssales-dev `
     --src ./publish.zip
   ```

3. **Ejecutar migraciones** (EF Core):
   - Configura la connection string en el App Service
   - Ejecuta migraciones desde tu máquina o en el startup

### Si desplegaste con -SqlOnly

1. Actualiza `appsettings.json` con la connection string mostrada:
   ```
   Server=<sql-server>.database.windows.net;Database=ProductsSalesDb;User ID=sqladmin;Password=TuPassword;Encrypt=True;TrustServerCertificate=False;
   ```

2. Ejecuta las migraciones:
   ```powershell
   dotnet ef database update --project ProductsSales.Infrastructure --startup-project ProductsSales.Api
   ```

3. Ejecuta la API localmente — se conectará a Azure SQL.

## Solución de problemas

### "Provisioning is restricted in this region"
Azure SQL no está disponible en la región. Usa otra: `-Location eastus2`, `-Location westus2`, `-Location centralus`, etc.

### "Invalid deployment location. The deployment 'MAIN' already exists"
Elimina el despliegue anterior:
```powershell
az deployment sub delete --name main
```
El script usa nombres únicos por defecto (`productssales-YYYYMMDD-HHmmss`) para evitar este conflicto.

### "Current Limit (Basic VMs): 0" o "Current Limit (Free VMs): 0"
La suscripción no tiene cuota para App Service. Opciones:
1. **Desplegar solo SQL**: `.\deploy.ps1 -SqlPassword "xxx" -SqlOnly`
2. **Solicitar cuota**: Azure Portal → Suscripciones → Usage + quotas → buscar "App Service" o "Free VMs" → Request increase

### Conflictos con resource group
Si un despliegue falla a mitad, elimina el grupo de recursos antes de reintentar:
```powershell
az group delete --name rg-productssales-dev --yes --no-wait
# Esperar 1-2 minutos antes de redeploy
```

## Estructura

```
bicep/
├── main.bicep          # Orquestador principal (RG + SQL + App Service opcional)
├── main.bicepparam     # Parámetros por defecto (formato Bicep)
├── deploy.ps1          # Script de despliegue
├── README.md           # Esta documentación
└── modules/
    ├── sql-server.bicep   # Azure SQL Server + Database + Firewall
    └── app-service.bicep  # App Service Plan (F1 Free) + Web App
```

## Configuración por defecto

- **Región**: eastus2 (cambiada desde eastus por restricciones de aprovisionamiento)
- **App Service Plan**: F1 (Free) — siempreOn deshabilitado
- **SQL Database**: Basic tier
- **Deployment name**: Único por ejecución (`productssales-YYYYMMDD-HHmmss`)

## Seguridad

- **Nunca** incluyas contraseñas en `main.bicepparam` o en el código
- Los secretos se pasan por línea de comandos en el script
- Usa Azure Key Vault para producción

## Ambientes

Para distintos ambientes (dev, staging, prod):

```powershell
.\deploy.ps1 -SqlPassword "xxx" -Environment prod
```

Crea recursos como `rg-productssales-prod`, `api-productssales-prod`, etc.
