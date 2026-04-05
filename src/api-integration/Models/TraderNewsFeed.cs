namespace FxIntegrationApi.Models;

public class TraderNewsFeed
{
    public int Id { get; set; }
    public int TraderId { get; set; }
    public string Headline { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string CurrencyPairs { get; set; } = string.Empty;
    public string Sentiment { get; set; } = "Neutral";
    public string Summary { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime PublishedAt { get; set; }

    public Trader Trader { get; set; } = null!;
}
