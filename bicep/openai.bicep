@description('The location for all resources')
param location string

@description('Environment name')
param environmentName string

// Azure OpenAI Service
resource openAI 'Microsoft.CognitiveServices/accounts@2023-05-01' = {
  name: 'openai-fx-${environmentName}'
  location: location
  kind: 'OpenAI'
  sku: {
    name: 'S0'
  }
  properties: {
    customSubDomainName: 'openai-fx-${environmentName}'
    publicNetworkAccess: 'Enabled'
  }
}

// GPT-4 Deployment
resource gpt4Deployment 'Microsoft.CognitiveServices/accounts/deployments@2023-05-01' = {
  parent: openAI
  name: 'gpt-4'
  properties: {
    model: {
      format: 'OpenAI'
      name: 'gpt-4'
      version: '0125-Preview'
    }
  }
  sku: {
    name: 'Standard'
    capacity: 10
  }
}

// Embedding Deployment
resource embeddingDeployment 'Microsoft.CognitiveServices/accounts/deployments@2023-05-01' = {
  parent: openAI
  name: 'text-embedding-ada-002'
  dependsOn: [gpt4Deployment]
  properties: {
    model: {
      format: 'OpenAI'
      name: 'text-embedding-ada-002'
      version: '2'
    }
  }
  sku: {
    name: 'Standard'
    capacity: 10
  }
}

output endpoint string = openAI.properties.endpoint
output openAIName string = openAI.name
