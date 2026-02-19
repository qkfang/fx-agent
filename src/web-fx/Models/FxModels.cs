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
}
