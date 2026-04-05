# Trading MCP Integration

This document describes the trading MCP endpoint that allows agents to execute trades.

## Architecture

```
Agent (FxTools.cs)
    ↓ HTTP POST
CRM Broker (McpController.cs)
    ↓ calls FxRateService
    ↓ executes trade
    ↓ HTTP POST
Trading Platform (TransactionsController.cs)
    ↓ updates transactions.json
    ↓ reflects in frontend
```

## MCP Endpoints

### Base URL
- Production: `https://<broker-url>/mcp`
- Development: `http://localhost:5269/mcp`

### Available Tools

#### 1. **fx_buy** - Execute Buy Order
Execute a market buy order for AUD/USD at the current ask price.

**Parameters:**
- `amount` (number, required): Amount in AUD to buy

**Request:**
```json
POST /mcp/call
{
  "tool": "fx_buy",
  "parameters": {
    "amount": 10000
  }
}
```

**Response:**
```json
{
  "success": true,
  "message": "Buy 10,000 AUD/USD executed at 0.6552",
  "data": {
    "id": "TXN000001",
    "type": "Buy",
    "currencyPair": "AUD/USD",
    "amount": 10000.00,
    "rate": 0.6552,
    "total": 6552.00,
    "source": "MCP",
    "timestamp": "2024-01-20T10:30:00Z"
  }
}
```

#### 2. **fx_sell** - Execute Sell Order
Execute a market sell order for AUD/USD at the current bid price.

**Parameters:**
- `amount` (number, required): Amount in AUD to sell

**Request:**
```json
POST /mcp/call
{
  "tool": "fx_sell",
  "parameters": {
    "amount": 5000
  }
}
```

**Response:**
```json
{
  "success": true,
  "message": "Sell 5,000 AUD/USD executed at 0.6550",
  "data": {
    "id": "TXN000002",
    "type": "Sell",
    "currencyPair": "AUD/USD",
    "amount": 5000.00,
    "rate": 0.6550,
    "total": 3275.00,
    "source": "MCP",
    "timestamp": "2024-01-20T10:31:00Z"
  }
}
```

#### 3. **fx_quote** - Get Current Quote
Get the current AUD/USD bid/ask quote with spread.

**Request:**
```json
POST /mcp/call
{
  "tool": "fx_quote",
  "parameters": {}
}
```

**Response:**
```json
{
  "success": true,
  "message": "OK",
  "data": {
    "currencyPair": "AUD/USD",
    "bid": 0.6550,
    "ask": 0.6552,
    "mid": 0.6551,
    "spread": 0.0002,
    "spreadPips": 2,
    "timestamp": "2024-01-20T10:30:00Z"
  }
}
```

#### 4. **fx_history** - Get Price History
Get OHLC candlestick price history for AUD/USD.

**Parameters:**
- `bars` (integer, optional): Number of candles to return (default 50, max 200)

**Request:**
```json
POST /mcp/call
{
  "tool": "fx_history",
  "parameters": {
    "bars": 100
  }
}
```

#### 5. **fx_market_status** - Get Market Status
Get current market status: trend, volatility, day high/low, active session.

**Request:**
```json
POST /mcp/call
{
  "tool": "fx_market_status",
  "parameters": {}
}
```

**Response:**
```json
{
  "success": true,
  "message": "OK",
  "data": {
    "trend": "Up",
    "volatility": 4.50,
    "dayOpen": 0.6545,
    "dayHigh": 0.6560,
    "dayLow": 0.6540,
    "dayChange": 0.0006,
    "dayChangePct": 0.092,
    "session": "Sydney / Tokyo"
  }
}
```

#### 6. **fx_set_trend** - Simulate Market Trend
Simulate a market trend event — triggers a price move in the given direction.

**Parameters:**
- `direction` (string, required): "up", "down", or "neutral"
- `strength` (integer, optional): Trend strength 0-100

**Request:**
```json
POST /mcp/call
{
  "tool": "fx_set_trend",
  "parameters": {
    "direction": "up",
    "strength": 75
  }
}
```

## Agent Integration

### FxTools.cs Methods

The agent exposes these MCP tools through C# methods:

```csharp
// Execute a buy trade
string result = ExecuteBuy(accountId: 1, lots: 0.5m);

// Execute a sell trade
string result = ExecuteSell(accountId: 1, lots: 0.5m);

// Get current quote
string quote = GetFxQuote();

// Get market status
string status = GetMarketStatus();

// Get price history
string history = GetPriceHistory(bars: 50);
```

## Transaction Flow

1. **Agent** calls `ExecuteBuy(1, 0.5m)` or `ExecuteSell(1, 0.5m)`
2. **HTTP Request** to broker API: `POST /api/accounts/1/buy` or `POST /api/accounts/1/sell`
3. **Broker** (AccountService) executes trade via FxRateService
4. **FxRateService** creates TransactionRecord
5. **Broker** pushes transaction to Trading Platform: `POST /api/transactions`
6. **Trading Platform** (TransactionsController) receives transaction
7. **FxDataService** updates `transactions.json` file
8. **Frontend** reflects updated transactions on page refresh or auto-update

## Configuration

### Broker (crm-broker)
```json
{
  "TradingPlatformUrl": "http://localhost:5249"
}
```

### Trading Platform (trading-platform)
No configuration needed - listens on port 5249 by default.

## Testing

### Test Buy Trade
```bash
curl -X POST http://localhost:5269/mcp/call \
  -H "Content-Type: application/json" \
  -d '{
    "tool": "fx_buy",
    "parameters": {
      "amount": 10000
    }
  }'
```

### Test Sell Trade
```bash
curl -X POST http://localhost:5269/mcp/call \
  -H "Content-Type: application/json" \
  -d '{
    "tool": "fx_sell",
    "parameters": {
      "amount": 5000
    }
  }'
```

### Verify in Trading Platform
1. Navigate to `http://localhost:5249`
2. Check the "Buy & Sell Activity" table
3. Verify the new transaction appears with correct details

## Notes

- All trades are executed at market prices (Buy at Ask, Sell at Bid)
- Transactions are immediately reflected in both broker and trading platform
- The trading platform may not be running - broker handles this gracefully
- All amounts are in AUD
- Exchange rate updates every second via tick simulation
