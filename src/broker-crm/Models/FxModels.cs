namespace FxWebApi.Models
{
    public class FxRate
    {
        public string CurrencyPair { get; set; } = "AUD/USD";
        public decimal Rate { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class FxQuote
    {
        public string CurrencyPair { get; set; } = "AUD/USD";
        public decimal Bid { get; set; }
        public decimal Ask { get; set; }
        public decimal Mid { get; set; }
        public decimal Spread { get; set; }
        public int SpreadPips { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class OhlcCandle
    {
        public DateTime Timestamp { get; set; }
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }
        public long Volume { get; set; }
    }

    public class OrderBookLevel
    {
        public decimal Price { get; set; }
        public decimal Size { get; set; }
        public decimal Total { get; set; }
    }

    public class OrderBook
    {
        public string CurrencyPair { get; set; } = "AUD/USD";
        public List<OrderBookLevel> Bids { get; set; } = new();
        public List<OrderBookLevel> Asks { get; set; } = new();
        public DateTime Timestamp { get; set; }
    }

    public class MarketStatus
    {
        public string Trend { get; set; } = "Neutral";
        public double Volatility { get; set; }
        public decimal DayOpen { get; set; }
        public decimal DayHigh { get; set; }
        public decimal DayLow { get; set; }
        public decimal DayChange { get; set; }
        public decimal DayChangePct { get; set; }
        public string Session { get; set; } = "Sydney";
    }

    public class TrendRequest
    {
        public string Direction { get; set; } = "neutral"; // "up", "down", "neutral"
        public int Strength { get; set; } = 50;           // 0-100
    }

    public class TransactionRecord
    {
        public string Id { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string CurrencyPair { get; set; } = "AUD/USD";
        public decimal Amount { get; set; }
        public decimal Rate { get; set; }
        public decimal Total { get; set; }
        public string Source { get; set; } = "API";
        public DateTime Timestamp { get; set; }
    }

    public class FxTransaction
    {
        public string Type { get; set; } = string.Empty;
        public string CurrencyPair { get; set; } = "AUD/USD";
        public decimal Amount { get; set; }
        public decimal Rate { get; set; }
    }

    public class FxTransactionResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public FxTransaction? Transaction { get; set; }
        public TransactionRecord? Record { get; set; }
    }

    public class McpRequest
    {
        public string Action { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string? CurrencyPair { get; set; }
    }

    public class McpToolCall
    {
        public string Tool { get; set; } = string.Empty;
        public Dictionary<string, object> Parameters { get; set; } = new();
    }

    public class McpResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public object? Data { get; set; }
    }

    public class Account
    {
        public int Id { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string AccountType { get; set; } = "Standard";
        public string Status { get; set; } = "Active";
        public decimal Balance { get; set; }
        public decimal Leverage { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class Position
    {
        public string PositionId { get; set; } = string.Empty;
        public int AccountId { get; set; }
        public string CurrencyPair { get; set; } = "AUD/USD";
        public string Type { get; set; } = string.Empty; // Buy or Sell
        public decimal Lots { get; set; }
        public decimal OpenRate { get; set; }
        public decimal CurrentRate { get; set; }
        public decimal PnL { get; set; }
        public decimal Margin { get; set; }
        public DateTime OpenTime { get; set; }
    }

    public class BalanceSheet
    {
        public int AccountId { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string AccountType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public decimal Equity { get; set; }
        public decimal OpenPnL { get; set; }
        public decimal Margin { get; set; }
        public decimal FreeMargin { get; set; }
        public decimal MarginLevel { get; set; }
        public List<Position> OpenPositions { get; set; } = new();
        public List<AccountTransaction> RecentTransactions { get; set; } = new();
    }

    public class AccountTransaction
    {
        public string TransactionId { get; set; } = string.Empty;
        public int AccountId { get; set; }
        public string Type { get; set; } = string.Empty;
        public string CurrencyPair { get; set; } = string.Empty;
        public decimal Lots { get; set; }
        public decimal Rate { get; set; }
        public decimal PnL { get; set; }
        public decimal BalanceAfter { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class TradeRequest
    {
        public string CurrencyPair { get; set; } = "AUD/USD";
        public decimal Lots { get; set; }
    }

    public class AccountSummary
    {
        public int Id { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string AccountType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public decimal Equity { get; set; }
        public decimal OpenPnL { get; set; }
        public int OpenPositionsCount { get; set; }
        public decimal Leverage { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>Lead notification received from Research Analytics when a customer reads an article.</summary>
    public class LeadNotification
    {
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string UserCompany { get; set; } = string.Empty;
        public int? ArticleId { get; set; }
        public string ArticleTitle { get; set; } = string.Empty;
        public int TimeSpentSeconds { get; set; }
        public string SessionId { get; set; } = string.Empty;
        public DateTime ReceivedAt { get; set; }
    }

    /// <summary>Trade settlement payload sent to the Trading Platform after a trade executes.</summary>
    public class TradeSettlementRequest
    {
        public string Type { get; set; } = string.Empty;
        public string CurrencyPair { get; set; } = "AUD/USD";
        public decimal Amount { get; set; }
        public decimal Rate { get; set; }
        public decimal Total { get; set; }
        public string Source { get; set; } = "BrokerCRM";
        public DateTime DateTime { get; set; }
    }

    /// <summary>Trade request submitted through the Orora trading popup in Research Analytics.</summary>
    public class OroraTradeRequest
    {
        /// <summary>Trade direction: "Buy" or "Sell".</summary>
        public string Direction { get; set; } = string.Empty;
        /// <summary>Currency pair (e.g. AUD/USD).</summary>
        public string CurrencyPair { get; set; } = "AUD/USD";
        /// <summary>Volume in lots.</summary>
        public decimal Lots { get; set; }
        /// <summary>CRM account ID to record the trade against.</summary>
        public int AccountId { get; set; }
        /// <summary>Customer name for the CRM note (optional).</summary>
        public string CustomerName { get; set; } = string.Empty;
    }

    /// <summary>Result returned by the Orora trade endpoint.</summary>
    public class OroraTradeResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Direction { get; set; } = string.Empty;
        public string CurrencyPair { get; set; } = string.Empty;
        public decimal Lots { get; set; }
        public decimal Rate { get; set; }
        public string TransactionId { get; set; } = string.Empty;
    }
}
