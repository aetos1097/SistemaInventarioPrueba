// ProductsSales - Infraestructura como Código (Bicep)
// Despliega: Resource Group, Azure SQL, App Service
targetScope = 'subscription'

param location string = 'eastus2'
param environment string = 'dev'
param projectName string = 'productssales'
param deployAppService bool = true  // false = solo SQL (cuando no hay cuota para App Service)

// Parámetros sensibles - pasar en tiempo de deploy (nunca en código)
@secure()
param sqlAdminPassword string
@secure()
param jwtSecretKey string

// 1. Resource Group
resource rg 'Microsoft.Resources/resourceGroups@2023-07-01' = {
  name: 'rg-${projectName}-${environment}'
  location: location
}

// 2. SQL Server + Database
module sql 'modules/sql-server.bicep' = {
  scope: rg
  name: 'sql-deployment'
  params: {
    location: location
    sqlServerName: 'sql-${projectName}-${environment}'
    databaseName: 'ProductsSalesDb'
    administratorLogin: 'sqladmin'
    administratorLoginPassword: sqlAdminPassword
  }
}

var connectionString = 'Server=tcp:${sql.outputs.sqlServerFqdn},1433;Initial Catalog=${sql.outputs.databaseName};Persist Security Info=False;User ID=${sql.outputs.administratorLogin};Password=${sqlAdminPassword};MultipleActiveResultSets=True;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;'

// 3. App Service (API) - solo si deployAppService=true
module webApp 'modules/app-service.bicep' = if (deployAppService) {
  scope: rg
  name: 'webapp-deployment'
  params: {
    location: location
    appServicePlanName: 'asp-${projectName}-${environment}'
    webAppName: 'api-${projectName}-${environment}'
    connectionString: connectionString
    jwtSecretKey: jwtSecretKey
  }
}

output webAppUrl string = deployAppService ? webApp.outputs.webAppUrl : ''
output webAppName string = deployAppService ? webApp.outputs.webAppName : ''
output resourceGroupName string = rg.name
output sqlServerFqdn string = sql.outputs.sqlServerFqdn
