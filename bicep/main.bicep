@description('Base name used to derive all resource names')
param baseName string = 'fxag'

@description('Azure region for all resources')
param location string = 'centralus'

param azureAIFoundryEndpoint string = 'https://fxag-foundry.openai.azure.com'
param azureAIFoundryDeployment string = 'gpt-4.1'
param azureAIFoundryTenantId string = '9d2116ce-afe6-4ce8-8bc3-c7c7b69856c2'

param principals array = []

var uniqueSuffix = uniqueString(resourceGroup().id)
var commonTags = {
  SecurityControl: 'Ignore'
}
var keyVaultName = '${baseName}-kv'
var storageAccountName = '${baseName}st'
var appInsightsName = '${baseName}-appi'
var logAnalyticsWorkspaceName = '${baseName}-log'
var webAppPlanName = '${baseName}-asp'
var staticWebAppName = '${baseName}-swa'
var foundryName = '${baseName}-foundry'

resource appServicePlan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: webAppPlanName
  location: location
  kind: 'linux'
  sku: {
    name: 'S1'
    tier: 'Standard'
  }
  properties: {
    reserved: true
  }
}

resource autoscale 'Microsoft.Insights/autoscalesettings@2022-10-01' = {
  name: '${webAppPlanName}-autoscale'
  location: location
  properties: {
    targetResourceUri: appServicePlan.id
    enabled: true
    profiles: [
      {
        name: 'defaultProfile'
        capacity: {
          minimum: '1'
          maximum: '10'
          default: '1'
        }
        rules: [
          {
            metricTrigger: {
              metricName: 'CpuPercentage'
              metricResourceUri: appServicePlan.id
              timeGrain: 'PT1M'
              statistic: 'Average'
              timeWindow: 'PT5M'
              timeAggregation: 'Average'
              operator: 'GreaterThan'
              threshold: 70
            }
            scaleAction: {
              direction: 'Increase'
              type: 'ChangeCount'
              value: '1'
              cooldown: 'PT5M'
            }
          }
          {
            metricTrigger: {
              metricName: 'CpuPercentage'
              metricResourceUri: appServicePlan.id
              timeGrain: 'PT1M'
              statistic: 'Average'
              timeWindow: 'PT5M'
              timeAggregation: 'Average'
              operator: 'LessThan'
              threshold: 30
            }
            scaleAction: {
              direction: 'Decrease'
              type: 'ChangeCount'
              value: '1'
              cooldown: 'PT5M'
            }
          }
        ]
      }
    ]
  }
}

module azureFoundry 'modules/foundry.bicep' = {
  name: 'foundryDeployment'
  params: {
    name: foundryName
    location: location
    tags: commonTags
    webAppPrincipalId: crmBrokerApp.outputs.principalId
    principals: principals
  }
}

module keyVault 'modules/keyvault.bicep' = {
  name: 'keyVaultDeployment'
  params: {
    name: keyVaultName
    location: location
  }
}

module storageAccount 'modules/storage.bicep' = {
  name: 'storageAccountDeployment'
  params: {
    name: storageAccountName
    location: location
    tags: commonTags
    webAppPrincipalId: crmBrokerApp.outputs.principalId
    principals: principals
  }
}

module appInsights 'modules/appinsights.bicep' = {
  name: 'appInsightsDeployment'
  params: {
    name: appInsightsName
    location: location
    workspaceName: logAnalyticsWorkspaceName
  }
}

module crmBrokerApp 'modules/webapp.bicep' = {
  name: 'crmBrokerDeployment'
  params: {
    name: '${baseName}-broker'
    location: location
    appServicePlanId: appServicePlan.id
    appInsightsConnectionString: appInsights.outputs.connectionString
    extraAppSettings: [
      { name: 'AzureAIFoundry__Endpoint', value: azureAIFoundryEndpoint }
      { name: 'AzureAIFoundry__Deployment', value: azureAIFoundryDeployment }
      { name: 'AzureAIFoundry__TenantId', value: azureAIFoundryTenantId }
      { name: 'AzureStorage__AccountName', value: storageAccountName }
    ]
  }
}

module fxAgentApp 'modules/webapp.bicep' = {
  name: 'fxAgentDeployment'
  params: {
    name: '${baseName}-agent'
    location: location
    appServicePlanId: appServicePlan.id
    appInsightsConnectionString: appInsights.outputs.connectionString
    extraAppSettings: [
      { name: 'AzureAIFoundry__Endpoint', value: azureAIFoundryEndpoint }
      { name: 'AzureAIFoundry__Deployment', value: azureAIFoundryDeployment }
      { name: 'AzureAIFoundry__TenantId', value: azureAIFoundryTenantId }
    ]
  }
}

module newsFeedApp 'modules/webapp.bicep' = {
  name: 'newsFeedDeployment'
  params: {
    name: '${baseName}-news'
    location: location
    appServicePlanId: appServicePlan.id
    appInsightsConnectionString: appInsights.outputs.connectionString
  }
}

module researchAnalyticsApp 'modules/webapp.bicep' = {
  name: 'researchAnalyticsDeployment'
  params: {
    name: '${baseName}-research'
    location: location
    appServicePlanId: appServicePlan.id
    appInsightsConnectionString: appInsights.outputs.connectionString
  }
}

module tradingPlatformApp 'modules/webapp.bicep' = {
  name: 'tradingPlatformDeployment'
  params: {
    name: '${baseName}-trading'
    location: location
    appServicePlanId: appServicePlan.id
    appInsightsConnectionString: appInsights.outputs.connectionString
  }
}

module staticWebApp 'modules/staticwebapp.bicep' = {
  name: 'staticWebAppDeployment'
  params: {
    name: staticWebAppName
    location: location
  }
}
