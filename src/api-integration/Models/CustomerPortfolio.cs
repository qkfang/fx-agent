namespace FxIntegrationApi.Models;

public class CustomerPortfolio
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string CurrencyPair { get; set; } = string.Empty;
    public string Direction { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal EntryRate { get; set; }
    public DateTime OpenedAt { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Open";

    public Customer Customer { get; set; } = null!;
}
