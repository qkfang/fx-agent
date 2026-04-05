namespace FxIntegrationApi.Models;

public class CustomerHistory
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string CurrencyPair { get; set; } = string.Empty;
    public string Direction { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal EntryRate { get; set; }
    public decimal ExitRate { get; set; }
    public decimal PnL { get; set; }
    public DateTime OpenedAt { get; set; }
    public DateTime ClosedAt { get; set; }
    public string Notes { get; set; } = string.Empty;

    public Customer Customer { get; set; } = null!;
}
