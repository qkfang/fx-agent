namespace FxWebUI.Models
{
    public class FxRate
    {
        public string CurrencyPair { get; set; } = string.Empty;
        public decimal Rate { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class Transaction
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty; // "Buy" or "Sell"
        public string CurrencyPair { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal Rate { get; set; }
        public decimal Total { get; set; }
        public DateTime DateTime { get; set; }
    }

    public class FundSummary
    {
        public decimal TotalBalance { get; set; }
        public decimal AudBalance { get; set; }
        public decimal UsdBalance { get; set; }
        public decimal TotalProfitLoss { get; set; }
    }
}
