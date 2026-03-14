# FX Agent – Python Multi-Agent Service

A Python server that orchestrates three AI agents using the **Microsoft Agent Framework** (Azure AI Foundry Agent Service) to automate FX trading decisions, execution, and customer communications.

## Architecture

```
src/fx-agent/
├── agents/
│   ├── analysis_agent.py   # Market research & BUY/SELL/HOLD recommendation
│   ├── trader_agent.py     # Trade execution via broker back-office MCP API
│   ├── comm_agent.py       # Broker approval requests & customer notifications
│   └── orchestrator.py     # Multi-agent workflow coordinator
├── tools/
│   ├── market_tools.py     # FX rate, news, portfolio fetchers
│   ├── trading_tools.py    # Buy/sell execution, transaction history
│   └── comm_tools.py       # Approval queue, notification store
├── static/
│   └── index.html          # Real-time agent dashboard (SPA)
├── main.py                 # FastAPI server entry point
├── config.py               # Settings loaded from environment / .env
├── requirements.txt
└── README.md
```

## Agents

### 📊 Analysis Agent
Researches the FX market by calling live data services:
- Fetches the current AUD/USD rate from the **broker back-office** service
- Reads news sentiment from the **news-feed** service
- Reviews the portfolio from the **trading platform**
- Produces a structured `BUY / SELL / HOLD` recommendation with reasoning

### 💬 Comm Agent
Bridges human brokers and customers:
- Submits trade recommendations to the broker for **human approval** (visible in the dashboard)
- Waits for the broker's decision before allowing trade execution
- Sends **customer notifications** after a trade is executed

### ⚡ Trader Agent
Executes approved trades:
- Checks the broker back-office **MCP service** status
- Calls `POST /mcp/fx` to execute a buy or sell order
- Verifies execution via transaction history

## Workflow

```
┌──────────────┐    BUY/SELL/HOLD    ┌─────────────────┐
│ Analysis     │─────────────────────▶ Comm Agent       │
│ Agent        │     recommendation  │ (broker approval)│
└──────────────┘                     └────────┬────────┘
                                              │ approved
                                     ┌────────▼────────┐
                                     │  Trader Agent   │
                                     │ (MCP execution) │
                                     └────────┬────────┘
                                              │ trade done
                                     ┌────────▼────────┐
                                     │  Comm Agent     │
                                     │ (notify customers)│
                                     └─────────────────┘
```

## Prerequisites

- Python 3.11+
- The existing .NET services running (broker-backoffice on port 5001, etc.)
- *(Optional)* Azure AI Foundry project for live LLM-powered agents

## Quick Start

### 1. Install dependencies

```bash
cd src/fx-agent
pip install -r requirements.txt
```

### 2. Configure (optional – for Azure AI Foundry)

Copy the template and fill in your Azure details:
```bash
cp .env.example .env
# Edit .env with your Azure AI connection string
```

Without Azure credentials the service runs in **demo mode** – it still calls
the real FX/news/trading APIs and simulates the agent reasoning flow.

### 3. Run the server

```bash
python main.py
# or
uvicorn main:app --host 0.0.0.0 --port 8000 --reload
```

Open [http://localhost:8000](http://localhost:8000) to see the dashboard.

## API Reference

| Method | Path | Description |
|--------|------|-------------|
| `GET`  | `/` | Agent execution dashboard |
| `GET`  | `/api/status` | Service health & mode |
| `POST` | `/api/workflow/run` | Trigger full agent workflow |
| `GET`  | `/api/workflow/history` | Past run summaries |
| `GET`  | `/api/approvals` | Pending broker approvals |
| `POST` | `/api/approvals/{id}` | Broker approve / reject |
| `GET`  | `/api/notifications` | Sent customer notifications |
| `GET`  | `/api/fx/rate` | Current AUD/USD rate (proxy) |
| `WS`   | `/ws` | Real-time agent event stream |

## Environment Variables

| Variable | Default | Description |
|----------|---------|-------------|
| `AZURE_AI_CONNECTION_STRING` | *(none)* | Azure AI Foundry connection string; if unset, demo mode is used |
| `AZURE_AI_MODEL` | `gpt-4o` | Model deployment name in your Foundry project |
| `BROKER_BACKOFFICE_URL` | `http://localhost:5001` | Broker back-office service base URL |
| `TRADING_PLATFORM_URL` | `http://localhost:5000` | Trading platform base URL |
| `NEWS_FEED_URL` | `http://localhost:5002` | News feed service base URL |

## Microsoft Agent Framework

This service uses the **Azure AI Foundry Agent Service** (via the `azure-ai-projects` SDK) as its agent framework. Each agent is defined with:

- A **system prompt** describing its role and response format
- **Tool functions** (OpenAI function-calling schema) that the agent can invoke
- Managed **threads** for stateful conversations

When `AZURE_AI_CONNECTION_STRING` is set, agents run on Azure AI Foundry with a real LLM. Without it, the service uses a deterministic demo mode that exercises all the same tool calls and produces realistic output.
