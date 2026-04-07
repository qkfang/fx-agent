namespace FxIntegrationApi.Models;

public class TraderSuggestion
{
    public int Id { get; set; }
    public int TraderId { get; set; }
    public int CustomerId { get; set; }
    public int ResearchArticleId { get; set; }
    public string Reasoning { get; set; } = string.Empty;
    public string RelevanceScore { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Trader Trader { get; set; } = null!;
    public Customer Customer { get; set; } = null!;
    public ResearchArticle ResearchArticle { get; set; } = null!;
}
