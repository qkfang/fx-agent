@description('Base name used to derive all resource names')
param baseName string = 'fxag'

@description('Azure region for all resources')
param location string = 'centralus'

param azureAIFoundryEndpoint string = 'https://fxag-foundry.openai.azure.com'
param azureAIFoundryDeployment string = 'gpt-4.1'
param azureAIFoundryTenantId string = '9d2116ce-afe6-4ce8-8bc3-c7c7b69856c2'

param fabricDatabaseConnectionString string = 'Data Source=zylcdhpgv7uezc6dy7d3ngcwyi-b5l3uoo37ijuxbntne4gq2ska4.database.fabric.microsoft.com,1433;Initial Catalog=fx_data_sqldb-af3802bf-c4ca-4c83-aa5a-366c574104d4;Multiple Active Result Sets=False;Connect Timeout=30;Encrypt=True;Trust Server Certificate=False;Authentication=Active Directory Default'

var eventHubFullyQualifiedNamespace = 'esehsyw4hwncugmy8frez7.servicebus.windows.net'
var eventHubName = 'es_fa73e095-515c-48fd-ad54-1ef70ad7bc34'

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
var fabricCapacityName = '${baseName}fabric'
var logicIntgName = '${baseName}-logic'

resource appServicePlan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: webAppPlanName
  location: location
  kind: 'linux'
  sku: {
    name: 'P1v3'
    tier: 'PremiumV3'
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
    appCommandLine: 'dotnet FxWebApi.dll'
    extraAppSettings: [
      { name: 'AzureAIFoundry__Endpoint', value: azureAIFoundryEndpoint }
      { name: 'AzureAIFoundry__Deployment', value: azureAIFoundryDeployment }
      { name: 'AzureAIFoundry__TenantId', value: azureAIFoundryTenantId }
      { name: 'AzureStorage__AccountName', value: storageAccountName }
      { name: 'IntegrationApiUrl', value: 'https://${baseName}-intg.azurewebsites.net' }
      { name: 'TradingPlatformUrl', value: 'https://${baseName}-trading.azurewebsites.net' }
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
    appCommandLine: 'dotnet FxAgent.dll'
    extraAppSettings: [
      { name: 'AZURE_AI_PROJECT_ENDPOINT', value: azureAIFoundryEndpoint }
      { name: 'AZURE_AI_MODEL_DEPLOYMENT_NAME', value: azureAIFoundryDeployment }
      { name: 'CRM_BROKER_URL', value: 'https://${baseName}-broker.azurewebsites.net' }
      { name: 'API_INTG_MCP_URL', value: 'https://${baseName}-intg.azurewebsites.net' }
      { name: 'TRADING_PLATFORM_MCP_URL', value: 'https://${baseName}-trading.azurewebsites.net' }
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
    appCommandLine: 'dotnet FxWebNews.dll'
    extraAppSettings: [
      { name: 'NewsPublish__EndpointUrl', value: 'https://${baseName}-research.azurewebsites.net/api/articles/receive' }
      { name: 'EventHub__FullyQualifiedNamespace', value: eventHubFullyQualifiedNamespace }
      { name: 'EventHub__EventHubName', value: eventHubName }
    ]
  }
}

module researchAnalyticsApp 'modules/webapp.bicep' = {
  name: 'researchAnalyticsDeployment'
  params: {
    name: '${baseName}-research'
    location: location
    appServicePlanId: appServicePlan.id
    appInsightsConnectionString: appInsights.outputs.connectionString
    appCommandLine: 'dotnet FxWebPortal.dll'
    extraAppSettings: [
      { name: 'IntegrationApi__BaseUrl', value: 'https://${baseName}-intg.azurewebsites.net' }
      { name: 'BrokerNotification__EndpointUrl', value: 'https://${baseName}-broker.azurewebsites.net/api/accounts/leads' }
      { name: 'Aurora__ApiUrl', value: 'https://${baseName}-broker.azurewebsites.net/api/aurora' }
      { name: 'Aurora__QuoteUrl', value: 'https://${baseName}-broker.azurewebsites.net/api/fx/quote' }
    ]
  }
}

module apiIntegrationApp 'modules/webapp.bicep' = {
  name: 'apiIntegrationDeployment'
  params: {
    name: '${baseName}-intg'
    location: location
    appServicePlanId: appServicePlan.id
    appInsightsConnectionString: appInsights.outputs.connectionString
    appCommandLine: 'dotnet FxIntegrationApi.dll'
    extraAppSettings: [
      { name: 'ConnectionStrings__FxDatabase', value: fabricDatabaseConnectionString }
    ]
  }
}

module tradingPlatformApp 'modules/webapp.bicep' = {
  name: 'tradingPlatformDeployment'
  params: {
    name: '${baseName}-trading'
    location: location
    appCommandLine: 'dotnet FxWebUI.dll'
    extraAppSettings: [
      { name: 'FX_API_URL', value: 'https://${baseName}-broker.azurewebsites.net' }
    ]
    appServicePlanId: appServicePlan.id
    appInsightsConnectionString: appInsights.outputs.connectionString
  }
}

module fabricCapacity 'modules/fabric.bicep' = {
  name: 'fabricCapacityDeployment'
  params: {
    name: fabricCapacityName
    location: location
    tags: commonTags
    adminMembers: concat(
      [
        'danielfang@MngEnvMCAP951655.onmicrosoft.com'
        'fabric@MngEnvMCAP951655.onmicrosoft.com'
      ]
    )
  }
}

module staticWebApp 'modules/staticwebapp.bicep' = {
  name: 'staticWebAppDeployment'
  params: {
    name: staticWebAppName
    location: location
  }
}

module logicIntg 'modules/logicapp-standard.bicep' = {
  name: 'logicIntgDeployment'
  params: {
    name: logicIntgName
    location: location
    tags: commonTags
    storageAccountName: storageAccountName
    appInsightsConnectionString: appInsights.outputs.connectionString
    newsFeedApiUrl: 'https://${baseName}-news.azurewebsites.net'
  }
}
