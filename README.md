# FX Agent - Complete Trading System

A comprehensive foreign exchange (FX) trading system built with .NET 8.0 and Microsoft Agent Framework, featuring real-time price simulation, transaction management, and news integration.

## Project Structure

```
fx-agent/
├── bicep/                  # Azure infrastructure as code
│   ├── main.bicep         # Main deployment template
│   ├── web-apps.bicep     # Web apps configuration
│   ├── logic-app.bicep    # Logic App for workflow orchestration
│   └── openai.bicep       # Azure OpenAI (Foundry) with GPT-4
├── src/
│   ├── web-ui/            # Trading dashboard UI
│   ├── web-fx/            # FX API and MCP service
│   └── web-news/          # News management system
└── README.md
```

## Components

### 1. Azure Resources (bicep/)

Infrastructure templates for deploying to Azure:
- **Resource Group**: `rg-fx-{environment}`
- **Web Apps**: Hosting for UI, API, and News applications
- **Logic App**: Workflow orchestration
- **Azure OpenAI**: GPT-4 and text-embedding-ada-002 models

**Deploy**: See [bicep/README.md](bicep/README.md) for deployment instructions.

### 2. Web UI Application (src/web-ui/FxWebUI)

ASP.NET Core Razor Pages application providing the main trading dashboard.

**Features**:
- Real-time FX rate display (AUD/USD)
- Transaction history grid
- Fund summary with P&L tracking
- Mock data loaded from JSON files

**Run**:
```bash
cd src/web-ui/FxWebUI
dotnet run
# Open browser to https://localhost:5000
```

### 3. Web FX Application (src/web-fx/FxWebApi)

ASP.NET Core Web API providing FX trading services.

**Features**:
- REST API for FX operations (`/api/fx/*`)
- MCP endpoint for agent integration (`/mcp/*`)
- Real-time price simulation (updates every 2 seconds)
- Interactive web UI with live chart

**API Endpoints**:
- `GET /api/fx/rate` - Get current FX rate
- `POST /api/fx/buy` - Execute buy transaction
- `POST /api/fx/sell` - Execute sell transaction
- `POST /mcp/fx` - MCP endpoint for buy/sell actions
- `GET /mcp/status` - MCP service status

**Run**:
```bash
cd src/web-fx/FxWebApi
dotnet run
# API: https://localhost:5001
# Web UI: https://localhost:5001/index.html
```

### 4. Web News Application (src/web-news/FxWebNews)

ASP.NET Core Razor Pages application for FX market news.

**Features**:
- News list page with good/bad news indicators
- Admin panel for adding/deleting articles
- JSON-based local storage
- No authentication (as specified)

**Run**:
```bash
cd src/web-news/FxWebNews
dotnet run
# News: https://localhost:5002
# Admin: https://localhost:5002/Admin
```

## Quick Start

### Prerequisites
- .NET 8.0 SDK
- Azure CLI (for deployment)

### Local Development

1. **Clone the repository**:
```bash
git clone https://github.com/qkfang/fx-agent.git
cd fx-agent
```

2. **Run Web FX API** (Terminal 1):
```bash
cd src/web-fx/FxWebApi
dotnet run --urls "http://localhost:5001"
```

3. **Run Web UI** (Terminal 2):
```bash
cd src/web-ui/FxWebUI
export FX_API_URL=http://localhost:5001  # Linux/Mac
# or set FX_API_URL=http://localhost:5001  # Windows
dotnet run --urls "http://localhost:5000"
```

4. **Run Web News** (Terminal 3):
```bash
cd src/web-news/FxWebNews
dotnet run --urls "http://localhost:5002"
```

### Access the Applications

- **Trading Dashboard**: http://localhost:5000
- **FX API**: http://localhost:5001
- **FX Status UI**: http://localhost:5001/index.html
- **News**: http://localhost:5002
- **News Admin**: http://localhost:5002/Admin

## Architecture

### Data Flow

```
┌─────────────┐         ┌──────────────┐         ┌─────────────┐
│   Web UI    │────────>│   FX API     │<────────│  MCP Agent  │
│  Dashboard  │         │  /api/fx/*   │         │   /mcp/*    │
└─────────────┘         └──────────────┘         └─────────────┘
      │                        │
      │                        │
      v                        v
┌─────────────┐         ┌──────────────┐
│  JSON Data  │         │ Price Engine │
│ (In-Memory) │         │ (2s updates) │
└─────────────┘         └──────────────┘
```

### FX Rate Simulation

The FX API includes a built-in price simulation engine that:
- Updates the AUD/USD rate every 2 seconds
- Randomizes price movements within realistic bounds (0.6000-0.7000)
- Provides consistent rates across all API calls

### MCP Integration

The Model Context Protocol (MCP) endpoint allows AI agents to:
- Execute buy/sell transactions programmatically
- Query current FX rates
- Integrate with Microsoft Agent Framework

## Data Storage

All applications use JSON files for data storage:
- **Web UI**: `Data/transactions.json`, `Data/fund.json`
- **Web News**: `Data/news.json`

Data is loaded into memory on startup and persisted back to JSON files when modified.

## Azure Deployment

Deploy all infrastructure and applications to Azure:

```bash
cd bicep
az login
az deployment sub create \
  --location eastus \
  --template-file main.bicep \
  --parameters main.parameters.json
```

See [bicep/README.md](bicep/README.md) for detailed deployment instructions.

## Development Notes

- **Port Configuration**: Applications use ports 5000 (UI), 5001 (FX API), 5002 (News)
- **CORS**: FX API has CORS enabled for local development
- **No Authentication**: As specified, no authentication is required
- **Mock Data**: All transactions and fund data are in-memory only

## License

This project is for demonstration purposes.