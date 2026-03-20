namespace FxWebPortal.Models;

public class CustomerSuggestion
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public string CurrencyPair { get; set; } = string.Empty;
    public string Direction { get; set; } = string.Empty; // "Buy" or "Sell"
    public string Analysis { get; set; } = string.Empty;
    public string Confidence { get; set; } = string.Empty; // High / Medium / Low
    public string SuggestedBy { get; set; } = string.Empty;
    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
}
