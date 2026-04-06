param name string
param location string
param appServicePlanId string
param appInsightsConnectionString string
param linuxFxVersion string = 'DOTNETCORE|9.0'
param appCommandLine string = ''
param extraAppSettings array = []

var baseAppSettings = [
  {
    name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
    value: appInsightsConnectionString
  }
  {
    name: 'ApplicationInsightsAgent_EXTENSION_VERSION'
    value: '~3'
  }
]

resource webApp 'Microsoft.Web/sites@2023-12-01' = {
  name: name
  location: location
  kind: 'app,linux'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: appServicePlanId
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: linuxFxVersion
      appCommandLine: appCommandLine
      minTlsVersion: '1.2'
      cors: {
        allowedOrigins: ['*']
      }
      appSettings: concat(baseAppSettings, extraAppSettings)
    }
  }
}

output id string = webApp.id
output name string = webApp.name
output defaultHostName string = webApp.properties.defaultHostName
output principalId string = webApp.identity.principalId
