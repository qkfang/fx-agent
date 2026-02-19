@description('The location for all resources')
param location string

@description('Environment name')
param environmentName string

// App Service Plan
resource appServicePlan 'Microsoft.Web/serverfarms@2022-03-01' = {
  name: 'asp-fx-${environmentName}'
  location: location
  sku: {
    name: 'B1'
    tier: 'Basic'
  }
  kind: 'linux'
  properties: {
    reserved: true
  }
}

// Web UI App
resource webUIApp 'Microsoft.Web/sites@2022-03-01' = {
  name: 'app-fx-ui-${environmentName}'
  location: location
  properties: {
    serverFarmId: appServicePlan.id
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|8.0'
      alwaysOn: true
    }
  }
}

// Web FX App (API and MCP Service)
resource webFXApp 'Microsoft.Web/sites@2022-03-01' = {
  name: 'app-fx-api-${environmentName}'
  location: location
  properties: {
    serverFarmId: appServicePlan.id
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|8.0'
      alwaysOn: true
    }
  }
}

// Web News App
resource webNewsApp 'Microsoft.Web/sites@2022-03-01' = {
  name: 'app-fx-news-${environmentName}'
  location: location
  properties: {
    serverFarmId: appServicePlan.id
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|8.0'
      alwaysOn: true
    }
  }
}

output webAppNames object = {
  ui: webUIApp.name
  fx: webFXApp.name
  news: webNewsApp.name
}
