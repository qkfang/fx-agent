# End-to-End Demo Playbook: Middle East War News → AUD/USD Trade

## Scenario Overview

A breaking news story about conflict in the Middle East triggers a chain of events across five interconnected systems — from news publication to a fully settled AUD/USD foreign-exchange trade.

```
News Feed → Research Analytics → Customer View → Broker Back-Office → Trading Platform
```

| Step | System | Action |
|------|--------|--------|
| 1 | **News Feed** | Admin publishes "War in Middle East" article |
| 2 | **Research Analytics** | Auto-creates a Bearish AUD/USD research note |
| 3 | **Research Analytics** | Customer opens and reads the article |
| 4 | **Broker Back-Office** | Receives customer lead; broker reaches out |
| 5 | **Broker Back-Office** | Broker executes a Buy AUD/USD order for the customer |
| 6 | **Trading Platform** | Settled trade recorded; fund position updated |

---

## Prerequisites

Start all five services before running the demo:

```bash
# Terminal 1 – News Feed (port 5142)
cd src/news-feed
dotnet run --launch-profile http

# Terminal 2 – Research Analytics (port 5003)
cd src/research-analytics
dotnet run --launch-profile http

# Terminal 3 – Broker Back-Office (port 5269)
cd src/broker-backoffice
dotnet run --launch-profile http

# Terminal 4 – Trading Platform (port 5249)
cd src/trading-platform
dotnet run --launch-profile http

# Terminal 5 – FX Agent (port 8000, optional AI workflow)
cd src/fx-agent
pip install -r requirements.txt
uvicorn main:app --host 0.0.0.0 --port 8000 --reload
```

---

## Step-by-Step Demo

### Step 1 — News Feed Publishes War News

1. Open the News Feed admin panel: **http://localhost:5142/Admin**
2. Click **"New Article"** and fill in:
   - **Title:** `Middle East Conflict Escalates – Risk-Off Sentiment Spreads to FX Markets`
   - **Summary:** `Escalating conflict in the Middle East has triggered risk-off flows, weighing on commodity-linked currencies including the Australian dollar.`
   - **Content:** *(paste the sample below)*
   - **Type:** `Bad`
   - **Category:** `FX`
   - **Author:** `FX News Team`

   **Sample Content:**
   ```
   Heightened military activity in the Middle East has sent shockwaves through global
   financial markets. Safe-haven assets including the US dollar and gold surged as
   investors fled risk. The Australian dollar, highly sensitive to commodity sentiment
   and global risk appetite, fell sharply against the USD. Analysts warn that a
   sustained conflict could further dampen AUD/USD as oil price uncertainty and
   supply-chain disruptions weigh on Australian export revenues.
   ```

3. Click **"Save"**, then click **"Publish"**.

**What happens:** The news-feed `NewsPublishService` sends an HTTP POST to Research Analytics at `http://localhost:5003/api/articles/receive` with the article payload.

---

### Step 2 — Research Analytics Receives News & Creates Research Note

The Research Analytics service automatically:
- Converts the incoming news article into a **Bearish** research note
- Tags it with `FX, NewsAlert, Risk`
- Sets status to **Published** so it is immediately visible to customers

**Verify:**
1. Open Research Analytics: **http://localhost:5003**
2. The new article *"Middle East Conflict Escalates…"* should appear at the top of the feed with a **Bearish** sentiment badge.

---

### Step 3 — Customer Reads the Research Article & Submits Lead

1. Click on the new article to open it.
2. Read the research note.
3. In the **"Stay Ahead of the Market"** sidebar form, enter:
   - **Your Name:** `James Wilson`
   - **Work Email:** `james.wilson@email.com`
   - **Company:** `Wilson Capital`
4. Click **"Get Research Updates"**.

**What happens:** The tracker fires a POST to `/api/track` with the customer's email and the article ID. Research Analytics then fires a non-blocking notification to Broker Back-Office at `http://localhost:5269/api/leads`.

---

### Step 4 — Broker Back-Office Receives the Lead & Reaches Out

1. Open Broker Back-Office: **http://localhost:5269**
2. Navigate to **Swagger UI** at `http://localhost:5269/swagger`
3. Call `GET /api/leads` to see the incoming lead:
   ```json
   [
     {
       "userName": "James Wilson",
       "userEmail": "james.wilson@email.com",
       "userCompany": "Wilson Capital",
       "articleId": 5,
       "articleTitle": "Middle East Conflict Escalates...",
       "timeSpentSeconds": 45,
       "receivedAt": "2026-03-15T00:42:00Z"
     }
   ]
   ```

4. The broker identifies the customer as **Account FX10001 – James Wilson** and decides to act on the Bearish signal by buying AUD/USD (buying AUD now at a lower price ahead of expected recovery, or hedging the client's USD exposure).

> **Broker note:** In the demo scenario the broker executes a Buy order for James Wilson's account to take advantage of the depressed AUD/USD rate caused by the Middle East news.

---

### Step 5 — Broker Executes Buy AUD/USD via Back-Office

Using the Swagger UI or the back-office frontend:

**Via Swagger:**
```
POST /api/accounts/1/buy
{
  "lots": 1.0
}
```

**Via cURL:**
```bash
curl -X POST http://localhost:5269/api/accounts/1/buy \
  -H "Content-Type: application/json" \
  -d '{"lots": 1.0}'
```

**Expected response:**
```json
{
  "success": true,
  "message": "Buy 1 lots AUD/USD at 0.6520 executed successfully",
  "transaction": {
    "type": "Buy",
    "currencyPair": "AUD/USD",
    "amount": 1.0,
    "rate": 0.6520
  }
}
```

**What happens:** After the trade is executed in the broker's account system, AccountService fires an HTTP POST to the Trading Platform at `http://localhost:5249/api/trades` to settle the trade.

---

### Step 6 — Trade Settles on Trading Platform

1. Open the Trading Platform: **http://localhost:5249**
2. The new **Buy AUD/USD** transaction is visible in the transaction history table.
3. The fund's AUD balance has increased and the transaction log shows the new entry with current market rate.

**Verify via API:**
```bash
# Get portfolio summary
curl http://localhost:5249/api/portfolio

# Get full transaction list
curl http://localhost:5249/api/trades
```

---

## Optional: AI Agent Workflow

The FX Agent (port 8000) can run the full scenario autonomously using AI:

1. Open FX Agent UI: **http://localhost:8000**
2. Click **"Run Workflow"** to trigger the orchestrator.
3. The AI agents will:
   - **Analysis Agent** — fetch news and FX rates; produce a BUY/SELL/HOLD recommendation
   - **Comm Agent** — request broker approval via the dashboard
   - **Trader Agent** — execute the approved trade via the Broker Back-Office MCP endpoint
   - **Comm Agent** — send customer notifications

**Approve the trade** in the Approvals panel when prompted.

---

## Integration Architecture

```
┌─────────────────────────────────────────────────────────────────────────┐
│                         FX Demo Platform                                 │
│                                                                         │
│  ┌──────────────┐    POST /api/articles/receive    ┌─────────────────┐  │
│  │  News Feed   │ ──────────────────────────────► │   Research      │  │
│  │  :5142       │                                  │   Analytics     │  │
│  └──────────────┘                                  │   :5003         │  │
│                                                    └────────┬────────┘  │
│                                                             │           │
│                                          POST /api/leads    │           │
│                                          (customer email)   ▼           │
│  ┌──────────────┐    POST /api/trades   ┌─────────────────┐            │
│  │  Trading     │ ◄──────────────────── │   Broker        │            │
│  │  Platform    │                       │   Back-Office   │            │
│  │  :5249       │                       │   :5269         │            │
│  └──────────────┘                       └────────▲────────┘            │
│                                                   │                     │
│                                    MCP /mcp/fx    │                     │
│  ┌──────────────────────────────────────────────┐ │                     │
│  │  FX Agent (Python/FastAPI) :8000             │─┘                     │
│  │  Analysis Agent → Comm Agent → Trader Agent  │                       │
│  └──────────────────────────────────────────────┘                       │
└─────────────────────────────────────────────────────────────────────────┘
```

## Service URL Quick Reference

| Service | Local URL | Purpose |
|---------|-----------|---------|
| News Feed | http://localhost:5142 | Publish breaking news articles |
| Research Analytics | http://localhost:5003 | Customer-facing research portal |
| Broker Back-Office | http://localhost:5269 | FX trading API + account management |
| Trading Platform | http://localhost:5249 | Fund dashboard + settled trades |
| FX Agent | http://localhost:8000 | AI orchestration workflow |

## Key API Endpoints Added for Integration

| Endpoint | Service | Description |
|----------|---------|-------------|
| `POST /api/articles/receive` | Research Analytics | Accepts news from News Feed; auto-creates research note |
| `GET /api/leads` | Broker Back-Office | Lists customer leads generated from article reads |
| `POST /api/leads` | Broker Back-Office | Receives lead notification from Research Analytics |
| `POST /api/trades` | Trading Platform | Receives settled trade from Broker Back-Office |
| `GET /api/portfolio` | Trading Platform | Exposes fund summary to FX Agent |

---

## Troubleshooting

| Issue | Resolution |
|-------|-----------|
| Research note not created after publish | Check `NewsPublish:EndpointUrl` in `news-feed/appsettings.Development.json` points to `http://localhost:5003/api/articles/receive` |
| Broker lead not received | Check `BrokerNotification:EndpointUrl` in `research-analytics/appsettings.json` points to `http://localhost:5269/api/leads` |
| Trade not appearing on Trading Platform | Check `TradingPlatformUrl` in `broker-backoffice/appsettings.json` points to `http://localhost:5249` |
| FX Agent workflow fails | Ensure all .NET services are running; AI features require `AZURE_AI_CONNECTION_STRING` in `src/fx-agent/.env` |
