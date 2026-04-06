param name string
param location string
param tags object = {}
param appInsightsWorkspaceId string = ''

resource logicApp 'Microsoft.Logic/workflows@2019-05-01' = {
  name: name
  location: location
  tags: tags
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    state: 'Enabled'
    definition: {
      '$schema': 'https://schema.management.azure.com/providers/Microsoft.Logic/schemas/2016-06-01/workflowdefinition.json#'
      contentVersion: '1.0.0.0'
      parameters: {}
      triggers: {
        manual: {
          type: 'Request'
          kind: 'Http'
          inputs: {
            schema: {
              type: 'object'
              properties: {
                event: { type: 'string' }
                payload: { type: 'object' }
              }
            }
          }
        }
      }
      actions: {
        Response: {
          type: 'Response'
          kind: 'Http'
          inputs: {
            statusCode: 200
            body: {
              status: 'received'
              event: '@triggerBody()?[\'event\']'
            }
          }
          runAfter: {}
        }
      }
    }
    parameters: {}
  }
}

resource diagnostics 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = if (!empty(appInsightsWorkspaceId)) {
  name: '${name}-diag'
  scope: logicApp
  properties: {
    workspaceId: appInsightsWorkspaceId
    logs: [
      {
        category: 'WorkflowRuntime'
        enabled: true
      }
    ]
    metrics: [
      {
        category: 'AllMetrics'
        enabled: true
      }
    ]
  }
}

output id string = logicApp.id
output name string = logicApp.name
output principalId string = logicApp.identity.principalId
output callbackUrl string = listCallbackUrl('${logicApp.id}/triggers/manual', logicApp.apiVersion).value
