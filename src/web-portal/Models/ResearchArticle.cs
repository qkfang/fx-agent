namespace FxWebPortal.Models;

public class ResearchArticle
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public DateTime PublishedDate { get; set; }
    public string Status { get; set; } = "Draft"; // "Draft" or "Published"
    public string Tags { get; set; } = string.Empty; // comma-separated
    public string Sentiment { get; set; } = "Neutral"; // "Bullish", "Bearish", "Neutral"
}
