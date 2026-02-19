@description('The location for all resources')
param location string

@description('Environment name')
param environmentName string

// Storage Account for Logic App
resource storageAccount 'Microsoft.Storage/storageAccounts@2022-09-01' = {
  name: 'stfx${environmentName}${uniqueString(resourceGroup().id)}'
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
}

// App Service Plan for Logic App
resource logicAppPlan 'Microsoft.Web/serverfarms@2022-03-01' = {
  name: 'asp-logic-fx-${environmentName}'
  location: location
  sku: {
    name: 'WS1'
    tier: 'WorkflowStandard'
  }
  kind: 'elastic'
}

// Logic App (Standard)
resource logicApp 'Microsoft.Web/sites@2022-03-01' = {
  name: 'logic-fx-${environmentName}'
  location: location
  kind: 'functionapp,workflowapp'
  properties: {
    serverFarmId: logicAppPlan.id
    siteConfig: {
      netFrameworkVersion: 'v6.0'
      appSettings: [
        {
          name: 'AzureWebJobsStorage'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};AccountKey=${storageAccount.listKeys().keys[0].value};EndpointSuffix=core.windows.net'
        }
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~4'
        }
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: 'node'
        }
        {
          name: 'WEBSITE_NODE_DEFAULT_VERSION'
          value: '~18'
        }
        {
          name: 'APP_KIND'
          value: 'workflowApp'
        }
      ]
    }
  }
}

output logicAppName string = logicApp.name
output storageAccountName string = storageAccount.name
