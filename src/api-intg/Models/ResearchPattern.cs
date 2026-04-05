namespace FxIntegrationApi.Models;

public class ResearchPattern
{
    public int Id { get; set; }
    public string CurrencyPair { get; set; } = string.Empty;
    public string PatternName { get; set; } = string.Empty;
    public string Timeframe { get; set; } = string.Empty;
    public string Direction { get; set; } = string.Empty;
    public decimal ConfidenceScore { get; set; }
    public string Description { get; set; } = string.Empty;
    public string DetectedBy { get; set; } = string.Empty;
    public DateTime DetectedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string Status { get; set; } = "Active";
}
