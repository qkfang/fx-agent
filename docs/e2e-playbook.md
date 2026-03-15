# End-to-End Simulation Playbook: Middle East Conflict → AUD/USD Trade

## Overview

This playbook describes the full end-to-end simulation of an FX trading workflow
that spans **all five applications** in the repository. A major conflict erupts
in the Middle East; the event flows from news publication through research analytics,
customer engagement, broker approval, trade execution, and settlement.

**Scenario key:** `middle_east_war`

---

## All Five Services

| Service | App folder | Port | Role |
|---------|-----------|------|------|
| **FX Agent** (Python/FastAPI) | `src/fx-agent/` | 8000 | AI orchestrator – drives the workflow |
| **Broker Back-Office** (.NET) | `src/broker-backoffice/` | 5269 | FX rates, account management, MCP trade execution |
| **Research Analytics** (.NET) | `src/research-analytics/` | 5003 | Research note authoring and visitor tracking |
| **News Feed** (.NET) | `src/news-feed/` | 5142 | News article publishing |
| **Trading Platform** (.NET) | `src/trading-platform/` | 5249 | Fund summary and transaction history display |

---

## Architecture & Call Chain

```
┌─────────────────────────────────────────────────────┐
│              FX Agent (Python) :8000                │
│  Orchestrator → Analysis Agent → Trader Agent       │
│                               → Comm Agent          │
└──┬──────────────┬─────────────┬──────────────┬──────┘
   │              │             │              │
   ▼              ▼             ▼              ▼
:5142          :5003          :5269          :5249
News Feed   Research       Broker       Trading
Service     Analytics    Back-Office   Platform
POST /api/  POST /api/   GET  /api/    GET /api/
 news        articles     fx/rate       portfolio
            POST /api/   POST /mcp/fx  GET /api/
             track        GET  /api/    transactions
                          accounts
                          GET  /api/
                          fx/transactions
```

### Per-Phase Service Calls

| Phase | Python Tool Called | HTTP Call | Target Service |
|---|---|---|---|
| 0 – News Feed | `publish_news_article()` | `POST /api/news` | news-feed :5142 |
| 1 – Market Analysis | `get_market_news()` | `GET /api/news` | news-feed :5142 |
| 1 – Market Analysis | `get_fx_rate()` | `GET /api/fx/rate` | broker-backoffice :5269 |
| 1 – Market Analysis | `get_portfolio_summary()` | `GET /api/accounts` | broker-backoffice :5269 |
| 1 – Research Note | `publish_research_note()` | `POST /api/articles` | research-analytics :5003 |
| 2 – Customer View | `track_article_view()` | `POST /api/track` | research-analytics :5003 |
| 3 – Broker Approval | (in-process approval queue) | – | fx-agent |
| 4 – Trade Execution | `get_mcp_status()` | `GET /mcp/status` | broker-backoffice :5269 |
| 4 – Trade Execution | `execute_buy()` | `POST /mcp/fx` | broker-backoffice :5269 |
| 4 – Trade Execution | `get_transaction_history()` | `GET /api/fx/transactions` | broker-backoffice :5269 |
| 5 – Notification | (in-process notification store) | – | fx-agent |

---

## Workflow Phases

### Phase 0 – News Feed: Article Published

**Service called:** `news-feed` (`:5142`)  
**Endpoint:** `POST http://localhost:5142/api/news`

| Step | Actor | Action |
|------|-------|--------|
| 0.1 | fx-agent orchestrator | Calls `publish_news_article()` → `POST /api/news` on the news-feed service |
| 0.2 | news-feed service | Persists article to `Data/news.json`; article is visible at `http://localhost:5142` |
| 0.3 | fx-agent orchestrator | Emits `news_published` event with the returned article ID |

**Financial context:** Middle East conflict → oil supply fears → commodity prices rise → Australia
(major commodity exporter) benefits → AUD/USD initially dips on risk-off but medium-term outlook
is positive.

---

### Phase 1 – Research Analytics: Market Analysis & Research Note

**Services called:** `broker-backoffice` (`:5269`), `news-feed` (`:5142`), `research-analytics` (`:5003`)

| Step | Actor | Action |
|------|-------|--------|
| 1.1 | Analysis Agent | Calls `get_fx_rate()` → `GET /api/fx/rate` on broker-backoffice |
| 1.2 | Analysis Agent | Calls `get_market_news()` → `GET /api/news` on news-feed; maps `type: "Good/Bad"` → `sentiment: "positive/negative"` |
| 1.3 | Analysis Agent | Calls `get_portfolio_summary()` → `GET /api/accounts` on broker-backoffice |
| 1.4 | Analysis Agent | Synthesises: 3 positive vs 1 negative news items → **BUY** recommendation |
| 1.5 | fx-agent orchestrator | Calls `publish_research_note()` → `POST /api/articles` on research-analytics; article immediately published |
| 1.6 | research-analytics service | Persists note to `Data/articles.json`; visible at `http://localhost:5003` |

**Recommendation output:**
```json
{
  "recommendation": "BUY",
  "amount": 100000,
  "currency_pair": "AUD/USD",
  "current_rate": 0.655,
  "reasons": ["Rate below average", "3 positive news items", "Portfolio capacity available"],
  "risks":   ["Central bank announcement risk", "Low liquidity window"],
  "summary": "BUY AUD/USD at 0.6550. Positive sentiment + commodity tailwinds."
}
```

---

### Phase 2 – Customer Engagement: Article View Logged

**Service called:** `research-analytics` (`:5003`)  
**Endpoint:** `POST http://localhost:5003/api/track`

| Step | Actor | Action |
|------|-------|--------|
| 2.1 | fx-agent orchestrator | Calls `track_article_view()` → `POST /api/track` with customer session data |
| 2.2 | research-analytics service | Persists visitor log to `Data/visitors.json`; customer captured as a lead |
| 2.3 | fx-agent orchestrator | Emits `customer_view` event; logs broker outreach trigger |

**Customer simulated:** John Smith (`john.smith@smithcapital.com`) at Smith Capital, 187 seconds reading time.

The visitor log is visible in the research-analytics admin dashboard at:  
`http://localhost:5003/Admin/` (login: admin / 9999)

---

### Phase 3 – Broker Back-Office: Approval Request

**Service:** fx-agent in-process approval queue (REST endpoints on fx-agent)

| Step | Actor | Action |
|------|-------|--------|
| 3.1 | Comm Agent | Calls `request_broker_approval()` – creates pending record in-process |
| 3.2 | Broker (human) | Reviews trade details in the fx-agent dashboard at `http://localhost:8000` |
| 3.3 | Broker (human) | Approves via `POST http://localhost:8000/api/approvals/{approval_id}` |
| 3.4 | fx-agent | Unblocks the waiting workflow coroutine |

---

### Phase 4 – Trade Execution & Settlement

**Service called:** `broker-backoffice` (`:5269`)  
**Endpoints:** `GET /mcp/status`, `POST /mcp/fx`, `GET /api/fx/transactions`

| Step | Actor | Action |
|------|-------|--------|
| 4.1 | Trader Agent | Calls `get_mcp_status()` → `GET /mcp/status` on broker-backoffice |
| 4.2 | Trader Agent | Calls `execute_buy()` → `POST /mcp/fx` `{"action":"buy","amount":100000}` |
| 4.3 | broker-backoffice | Executes buy at current ask; records transaction in-memory; returns `TransactionRecord` |
| 4.4 | Trader Agent | Calls `get_transaction_history()` → `GET /api/fx/transactions` to verify settlement |
| 4.5 | Trader Agent | Returns execution report with transaction ID, rate, and timestamp |

Transaction history is also available at:  
`http://localhost:5249/api/transactions` (trading-platform static data)  
`http://localhost:5269/api/fx/transactions` (broker-backoffice live data ← preferred)

---

### Phase 5 – Customer Notification

**Service:** fx-agent in-process notification store

| Step | Actor | Action |
|------|-------|--------|
| 5.1 | Comm Agent | Calls `send_customer_notification()` – stores notification in-process |
| 5.2 | fx-agent | Notification retrievable via `GET http://localhost:8000/api/notifications` |
| 5.3 | Customer | Receives: *"Trade Alert: BUY 100,000 AUD/USD at 0.6550"* |

---

## Starting All Services

```bash
# Terminal 1 – Broker Back-Office
dotnet run --project src/broker-backoffice --launch-profile http
# → http://localhost:5269  (Swagger UI at /swagger)

# Terminal 2 – Research Analytics
dotnet run --project src/research-analytics --launch-profile http
# → http://localhost:5003  (Admin at /Admin, login: admin/9999)

# Terminal 3 – News Feed
dotnet run --project src/news-feed --launch-profile http
# → http://localhost:5142  (Admin at /Admin)

# Terminal 4 – Trading Platform
dotnet run --project src/trading-platform --launch-profile http
# → http://localhost:5249

# Terminal 5 – FX Agent (Python)
cd src/fx-agent
pip install -r requirements.txt
python main.py
# → http://localhost:8000
```

---

## Triggering the Simulation

### Step 1 – Start the Middle East War Scenario

```bash
curl -X POST http://localhost:8000/api/workflow/run \
  -H "Content-Type: application/json" \
  -d '{"scenario": "middle_east_war"}'
```

Response:
```json
{ "status": "started", "message": "Workflow started – connect to /ws for live events" }
```

### Step 2 – Watch Real-Time Events (WebSocket)

```bash
# Using wscat
wscat -c ws://localhost:8000/ws
```

Or open `http://localhost:8000` in a browser.

### Step 3 – Approve the Broker Trade (Human-in-the-Loop)

When the workflow emits `awaiting_approval`, retrieve the approval ID and approve:

```bash
# Get the pending approval ID
curl http://localhost:8000/api/approvals

# Approve (replace APPROVAL_ID)
curl -X POST http://localhost:8000/api/approvals/APPROVAL_ID \
  -H "Content-Type: application/json" \
  -d '{"decision": "approve", "notes": "Customer John Smith confirmed via phone"}'
```

### Step 4 – Verify Results Across All Services

```bash
# FX Agent – workflow history
curl http://localhost:8000/api/workflow/history

# FX Agent – customer notifications
curl http://localhost:8000/api/notifications

# Broker Back-Office – executed transactions (live)
curl http://localhost:5269/api/fx/transactions

# Broker Back-Office – account balances after trade
curl http://localhost:5269/api/accounts

# Research Analytics – published research notes
curl http://localhost:5003/api/articles

# News Feed – published news articles
curl http://localhost:5142/api/news

# Trading Platform – fund summary
curl http://localhost:5249/api/portfolio
```

---

## Expected Event Stream

```
workflow_start          → "Starting FX trading workflow (run XXXXXXXX)"
phase                   → "Phase 0: News Feed"
news_published          → POST /api/news → news-feed :5142
  └─ "Breaking: War erupts in Middle East – oil prices surge"
status                  → "News article published to news-feed service"
phase                   → "Phase 1: Research Analytics & Market Analysis"
status                  → "Analysis Agent started (demo mode)"
tool_call               → get_fx_rate       → GET /api/fx/rate       :5269
tool_result             → "Current rate: 0.655"
tool_call               → get_market_news   → GET /api/news          :5142
tool_result             → "Fetched 4 articles (3 positive, 1 negative)"
tool_call               → get_portfolio_summary → GET /api/accounts   :5269
tool_result             → "Portfolio data retrieved"
result                  → { "recommendation": "BUY", "amount": 100000, ... }
research_note           → POST /api/articles → research-analytics :5003
  └─ "AUD/USD – Buying Opportunity Amid Middle East Uncertainty"
recommendation          → "Recommendation: BUY AUD/USD 100,000"
phase                   → "Phase 2: Customer Engagement"
customer_view           → POST /api/track   → research-analytics :5003
  └─ "John Smith viewed research note (187s)"
broker_outreach         → "Broker notified to reach out to John Smith"
phase                   → "Phase 3: Broker Approval"
status                  → "Comm Agent requesting broker approval (demo mode)"
awaiting_approval       → "Waiting for broker approval via the dashboard…"
                         ←── Human approves via POST /api/approvals/XXXX
approval_decision       → "Broker decision: approve"
phase                   → "Phase 4: Trade Execution"
tool_call               → get_mcp_status    → GET /mcp/status        :5269
tool_result             → "MCP service status"
tool_call               → execute_buy       → POST /mcp/fx           :5269
tool_result             → "Trade executed: BUY 100,000 AUD/USD"
tool_call               → get_transaction_history → GET /api/fx/transactions :5269
tool_result             → "Settlement confirmed – transaction record returned"
result                  → { "execution_status": "executed", "action": "buy", ... }
phase                   → "Phase 5: Customer Notification"
tool_call               → send_customer_notification
tool_result             → "Notification sent: Trade Alert: BUY 100,000 AUD/USD"
result                  → "Customer notification sent (ID: XXXXXXXX)"
workflow_end            → "Workflow XXXXXXXX finished"
```

---

## Service API Quick Reference

### Broker Back-Office (`:5269`)
| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/fx/rate` | Current AUD/USD mid-price |
| `GET` | `/api/fx/transactions` | Recent trade records |
| `GET` | `/api/accounts` | All customer account summaries |
| `POST` | `/mcp/fx` | Execute buy/sell via MCP |
| `GET` | `/mcp/status` | MCP service health |
| `POST` | `/api/fx/trend` | Set simulated market trend |

### Research Analytics (`:5003`)
| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/articles` | List published research articles |
| `POST` | `/api/articles` | Create and publish a research note |
| `POST` | `/api/track` | Record visitor/customer engagement |

### News Feed (`:5142`)
| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/news` | List all news articles |
| `POST` | `/api/news` | Create a new news article |

### Trading Platform (`:5249`)
| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/portfolio` | Fund summary |
| `GET` | `/api/transactions` | Static transaction history |

### FX Agent (`:8000`)
| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/status` | Service health + all backend URLs |
| `POST` | `/api/workflow/run` | Start workflow (body: `{"scenario":"middle_east_war"}`) |
| `GET` | `/api/workflow/history` | Past run summaries |
| `GET` | `/api/approvals` | Pending broker approvals |
| `POST` | `/api/approvals/{id}` | Broker approve/reject |
| `GET` | `/api/notifications` | Sent customer notifications |
| `WS` | `/ws` | Real-time event stream |

---

## Running in Azure AI Foundry Mode

Set `AZURE_AI_CONNECTION_STRING` to use real LLM agents:

```bash
export AZURE_AI_CONNECTION_STRING="endpoint=https://...;subscription_id=...;resource_group=...;project_name=..."
cd src/fx-agent && python main.py
```

In Foundry mode the scenario context is passed to the LLM as part of the user prompt;
the LLM drives tool selection and recommendation synthesis, but the same real HTTP calls
are made to all five services.
