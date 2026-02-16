targetScope = 'subscription'

@description('The location for all resources')
param location string = 'eastus'

@description('Environment name')
param environmentName string = 'dev'

// Create resource group
resource rg 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: 'rg-fx-${environmentName}'
  location: location
}

// Deploy web apps
module webApps 'web-apps.bicep' = {
  scope: rg
  name: 'webApps'
  params: {
    location: location
    environmentName: environmentName
  }
}

// Deploy Logic App
module logicApp 'logic-app.bicep' = {
  scope: rg
  name: 'logicApp'
  params: {
    location: location
    environmentName: environmentName
  }
}

// Deploy Azure OpenAI (Foundry)
module openAI 'openai.bicep' = {
  scope: rg
  name: 'openAI'
  params: {
    location: location
    environmentName: environmentName
  }
}

output resourceGroupName string = rg.name
output webAppNames object = webApps.outputs.webAppNames
output logicAppName string = logicApp.outputs.logicAppName
output openAIEndpoint string = openAI.outputs.endpoint
