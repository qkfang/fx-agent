# End-to-End Simulation Playbook: Middle East Conflict → AUD/USD Trade

## Overview

This playbook describes the full end-to-end simulation of an FX trading workflow triggered by a geopolitical news event. It walks through every system component from news publication through to trade settlement and customer notification.

**Scenario:** A major conflict erupts in the Middle East. The news is picked up by the news-feed service, processed by research analytics into a customer-facing research note, and ultimately results in a broker-initiated AUD/USD buy trade on behalf of an interested customer.

---

## Architecture

```
News Feed Service  ──►  Research Analytics (fx-agent Analysis Agent)
                               │
                               ▼
                        Research Note published to customer portal
                               │
                               ▼
                        Customer views article  ──►  Broker outreach trigger
                               │
                               ▼
                        Broker Back-Office (broker approval)
                               │
                               ▼
                        Trading Platform  ──►  Trade executed & settled
                               │
                               ▼
                        Customer notification (Comm Agent)
```

---

## Workflow Phases

### Phase 0 – News Feed: Article Published

| Step | Actor | Action |
|------|-------|--------|
| 0.1 | News Feed Service | Publishes article: *"War erupts in Middle East: oil prices surge on supply fears"* |
| 0.2 | News Feed Service | Forwards article to Research Analytics queue |

**Simulated event type:** `news_published`

**Financial context:** Middle East conflict → oil supply fears → commodity prices rise → Australia (major commodity exporter) benefits → AUD/USD initially dips on risk-off but medium-term outlook is positive.

---

### Phase 1 – Research Analytics: Research Note Created

| Step | Actor | Action |
|------|-------|--------|
| 1.1 | Analysis Agent | Retrieves current AUD/USD rate via `get_fx_rate` |
| 1.2 | Analysis Agent | Fetches news from `get_market_news` — returns geopolitical news headlines |
| 1.3 | Analysis Agent | Reads portfolio summary via `get_portfolio_summary` |
| 1.4 | Analysis Agent | Synthesises: 3 positive signals (commodity windfall) vs 1 negative (USD safe-haven) |
| 1.5 | Analysis Agent | Produces research note with **BUY** recommendation for AUD/USD at current dip |
| 1.6 | Research Analytics | Publishes note: *"AUD/USD – Buying Opportunity Amid Middle East Uncertainty"* to customer portal |

**Simulated event types:** `tool_call`, `tool_result`, `result`, `research_note`

**Recommendation output (demo mode):**
```json
{
  "recommendation": "BUY",
  "amount": 100000,
  "currency_pair": "AUD/USD",
  "current_rate": 0.655,
  "reasons": [
    "Rate 0.6550 is below recent average – buying opportunity",
    "3 positive news items support AUD strength via commodity price rise",
    "Portfolio has capacity for additional AUD/USD exposure"
  ],
  "risks": [
    "Unexpected central bank announcement could reverse direction",
    "Prolonged conflict could dampen global risk appetite"
  ],
  "summary": "BUY AUD/USD at 0.6550. Positive news sentiment combined with current rate level drives this call."
}
```

---

### Phase 2 – Customer Engagement: Article View Triggers Broker Outreach

| Step | Actor | Action |
|------|-------|--------|
| 2.1 | Customer Portal | Customer **John Smith (CUST-001)** opens research note |
| 2.2 | Customer Portal | View event logged; engagement system detects research note view |
| 2.3 | Engagement System | Sends broker-outreach trigger: *"Customer CUST-001 read AUD/USD research note"* |
| 2.4 | Broker Notification | Broker receives alert to contact customer |

**Simulated event types:** `customer_view`, `broker_outreach`

---

### Phase 3 – Broker Back-Office: Approval Request

| Step | Actor | Action |
|------|-------|--------|
| 3.1 | Comm Agent | Formats approval request for the broker dashboard |
| 3.2 | Comm Agent | Calls `request_broker_approval` — creates pending record in approval queue |
| 3.3 | Broker (human) | Reviews recommendation in the FX Agent dashboard |
| 3.4 | Broker (human) | Approves trade via `POST /api/approvals/{approval_id}` with `{"decision": "approve"}` |
| 3.5 | Comm Agent | Receives approval decision; passes `approve` status to orchestrator |

**Simulated event types:** `tool_call`, `tool_result`, `approval_result`, `awaiting_approval`, `approval_decision`

> **Demo auto-approve:** In simulation mode the workflow waits for a human decision via the REST API.  
> To auto-approve without human interaction, send the approval HTTP request (see [Triggering the Simulation](#triggering-the-simulation)).

---

### Phase 4 – Trading Platform: Trade Execution & Settlement

| Step | Actor | Action |
|------|-------|--------|
| 4.1 | Trader Agent | Checks MCP service status via `get_mcp_status` |
| 4.2 | Trader Agent | Calls `execute_buy(amount=100000, currency_pair="AUD/USD")` |
| 4.3 | Broker Back-Office | Sends BUY order to trading platform via MCP endpoint (`POST /mcp/fx`) |
| 4.4 | Trading Platform | Executes trade at current market rate; returns transaction ID |
| 4.5 | Trader Agent | Verifies via `get_transaction_history` — confirms new transaction is recorded |
| 4.6 | Trader Agent | Returns execution report with transaction ID, rate, and timestamp |

**Simulated event types:** `tool_call`, `tool_result`, `result` (trade execution report)

**Trade settled** once transaction appears in `get_transaction_history` with status `executed`.

---

### Phase 5 – Customer Notification: Trade Confirmed

| Step | Actor | Action |
|------|-------|--------|
| 5.1 | Comm Agent | Formats customer trade-alert notification |
| 5.2 | Comm Agent | Calls `send_customer_notification` with trade details |
| 5.3 | Notification System | Stores notification; in production, dispatches via email/SMS |
| 5.4 | Customer | Receives: *"Trade Alert: BUY 100,000 AUD/USD at 0.6550"* |

**Simulated event types:** `tool_call`, `tool_result`, `result` (notification confirmation)

---

## Triggering the Simulation

### Prerequisites

Ensure the fx-agent Python service is running:

```bash
cd src/fx-agent
pip install -r requirements.txt
python main.py
# Service available at http://localhost:8000
```

### Step 1 – Start the Middle East War Scenario

```bash
curl -X POST http://localhost:8000/api/workflow/run \
  -H "Content-Type: application/json" \
  -d '{"scenario": "middle_east_war"}'
```

Response:
```json
{
  "status": "started",
  "message": "Workflow started – connect to /ws for live events"
}
```

### Step 2 – Watch Real-Time Events (WebSocket)

```bash
# Using wscat (npm install -g wscat)
wscat -c ws://localhost:8000/ws
```

Or open `http://localhost:8000` in a browser for the live dashboard.

### Step 3 – Approve the Broker Trade (Human-in-the-Loop)

When the workflow reaches Phase 3 and emits `awaiting_approval`, retrieve the approval ID and approve:

```bash
# Get pending approvals
curl http://localhost:8000/api/approvals

# Approve (replace APPROVAL_ID with the value from the response)
curl -X POST http://localhost:8000/api/approvals/APPROVAL_ID \
  -H "Content-Type: application/json" \
  -d '{"decision": "approve", "notes": "Customer John Smith confirmed via phone – proceed with BUY"}'
```

### Step 4 – Verify Results

```bash
# Check workflow history
curl http://localhost:8000/api/workflow/history

# Check customer notifications
curl http://localhost:8000/api/notifications
```

---

## Expected Event Stream

Below is the expected sequence of WebSocket events for the `middle_east_war` scenario:

```
workflow_start          → "Starting FX trading workflow (run XXXXXXXX)"
phase                   → "Phase 0: News Feed"
news_published          → "Breaking: War erupts in Middle East – AUD/USD under pressure"
status                  → "News article forwarded to Research Analytics for processing"
phase                   → "Phase 1: Research Analytics & Market Analysis"
status                  → "Analysis Agent started (demo mode)"
tool_call               → "Calling tool: get_fx_rate"
tool_result             → "Current rate: 0.655"
tool_call               → "Calling tool: get_market_news"
tool_result             → "Fetched 4 articles"
tool_call               → "Calling tool: get_portfolio_summary"
tool_result             → "Portfolio data retrieved"
result                  → { "recommendation": "BUY", "amount": 100000, ... }
research_note           → "Research note published: AUD/USD – Buying Opportunity Amid Middle East Uncertainty"
phase                   → "Phase 2: Customer Engagement"
customer_view           → "Customer 'John Smith' opened research note"
broker_outreach         → "Customer engagement triggered broker outreach for CUST-001"
recommendation          → "Recommendation: BUY AUD/USD 100,000"
phase                   → "Phase 3: Broker Approval"
status                  → "Comm Agent requesting broker approval (demo mode)"
tool_call               → "Calling tool: request_broker_approval"
awaiting_approval       → "Waiting for broker approval via the dashboard…"
approval_decision       → "Broker decision: approve"
phase                   → "Phase 4: Trade Execution"
status                  → "Trader Agent executing BUY 100,000 AUD/USD (demo mode)"
tool_call               → "Calling tool: get_mcp_status"
tool_result             → "MCP service: offline (demo fallback)"
tool_call               → "Calling tool: execute_buy"
tool_result             → "Trade executed: 100,000 AUD/USD"
tool_call               → "Calling tool: get_transaction_history"
tool_result             → "Transaction history retrieved – trade settled"
result                  → { "execution_status": "executed", "action": "buy", ... }
phase                   → "Phase 5: Customer Notification"
status                  → "Comm Agent sending customer notification (demo mode)"
tool_call               → "Calling tool: send_customer_notification"
tool_result             → "Notification sent: Trade Alert: BUY 100,000 AUD/USD"
result                  → "Customer notification sent successfully (ID: XXXXXXXX)"
workflow_end            → "Workflow XXXXXXXX finished"
```

---

## Available Scenarios

| Scenario Key | Description | Expected Recommendation |
|---|---|---|
| `middle_east_war` | Middle East conflict drives commodity prices up; AUD benefits as net exporter | **BUY** AUD/USD |
| *(default/empty)* | Standard demo mode with mixed news sentiment | BUY / SELL / HOLD (rate-dependent) |

---

## Service URLs

| Service | URL | Purpose |
|---------|-----|---------|
| FX Agent (Python) | `http://localhost:8000` | Main orchestrator + dashboard |
| Broker Back-Office (.NET) | `http://localhost:5001` | FX rates, MCP trade execution |
| Trading Platform (.NET) | `http://localhost:5000` | Portfolio, transaction history |
| News Feed (.NET) | `http://localhost:5002` | News articles |

---

## Running in Azure AI Foundry Mode

Set the `AZURE_AI_CONNECTION_STRING` environment variable to use real AI agents:

```bash
export AZURE_AI_CONNECTION_STRING="endpoint=https://...;subscription_id=...;resource_group=...;project_name=..."
cd src/fx-agent && python main.py
```

The same scenario parameter works in Foundry mode — the news context is passed to the LLM as part of the user message, and the LLM drives tool selection and recommendation synthesis.
