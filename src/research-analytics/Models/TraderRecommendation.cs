namespace FxWebPortal.Models;

public class TraderRecommendation
{
    public int Id { get; set; }
    public int TraderId { get; set; }
    public string CurrencyPair { get; set; } = string.Empty;
    public string Direction { get; set; } = string.Empty;
    public decimal TargetRate { get; set; }
    public decimal StopLoss { get; set; }
    public string Confidence { get; set; } = string.Empty;
    public string Rationale { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }

    public Trader Trader { get; set; } = null!;
}
