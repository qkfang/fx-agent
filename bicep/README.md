# Azure Bicep Templates for FX Agent

This folder contains Azure Bicep templates for deploying the FX Agent infrastructure.

## Resources Created

- **Resource Group**: `rg-fx-{environment}`
- **Web Apps**:
  - `app-fx-ui-{environment}`: Web UI application
  - `app-fx-api-{environment}`: FX API and MCP service
  - `app-fx-news-{environment}`: News application
- **Logic App**: `logic-fx-{environment}` for workflow orchestration
- **Azure OpenAI**: OpenAI service with GPT-4 and text-embedding-ada-002 deployments

## Deployment

### Prerequisites
- Azure CLI installed
- Azure subscription

### Deploy

```bash
# Login to Azure
az login

# Deploy to subscription
az deployment sub create \
  --location eastus \
  --template-file main.bicep \
  --parameters main.parameters.json
```

### Clean Up

```bash
# Delete resource group
az group delete --name rg-fx-dev --yes
```
