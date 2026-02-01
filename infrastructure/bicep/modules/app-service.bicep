// Módulo: Azure App Service para ProductsSales API
param location string
param appServicePlanName string
param webAppName string
@secure()
param connectionString string
@secure()
param jwtSecretKey string

resource appServicePlan 'Microsoft.Web/serverfarms@2022-09-01' = {
  name: appServicePlanName
  location: location
  sku: {
    name: 'F1'
    tier: 'Free'
  }
  kind: 'linux'
  properties: {
    reserved: true
  }
}

resource webApp 'Microsoft.Web/sites@2022-09-01' = {
  name: webAppName
  location: location
  kind: 'app,linux'
  properties: {
    serverFarmId: appServicePlan.id
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|8.0'
      alwaysOn: false
      ftpsState: 'FtpsOnly'
      minTlsVersion: '1.2'
      appSettings: [
        {
          name: 'ConnectionStrings__DefaultConnection'
          value: connectionString
        }
        {
          name: 'Jwt__SecretKey'
          value: jwtSecretKey
        }
        {
          name: 'Jwt__Issuer'
          value: 'ProductsSales.Api'
        }
        {
          name: 'Jwt__Audience'
          value: 'ProductsSales.Client'
        }
        {
          name: 'ASPNETCORE_ENVIRONMENT'
          value: 'Production'
        }
      ]
    }
    httpsOnly: true
  }
}

output webAppUrl string = 'https://${webApp.properties.defaultHostName}'
output webAppName string = webApp.name
output webAppHostName string = webApp.properties.defaultHostName
