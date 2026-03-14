namespace FxWebApi.Models
{
    public class FxRate
    {
        public string CurrencyPair { get; set; } = "AUD/USD";
        public decimal Rate { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class FxTransaction
    {
        public string Type { get; set; } = string.Empty; // "Buy" or "Sell"
        public string CurrencyPair { get; set; } = "AUD/USD";
        public decimal Amount { get; set; }
        public decimal Rate { get; set; }
    }

    public class FxTransactionResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public FxTransaction? Transaction { get; set; }
    }

    public class McpRequest
    {
        public string Action { get; set; } = string.Empty; // "buy" or "sell"
        public decimal Amount { get; set; }
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
}
