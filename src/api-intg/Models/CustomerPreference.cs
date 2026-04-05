namespace FxIntegrationApi.Models;

public class CustomerPreference
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string PreferredCurrencyPairs { get; set; } = string.Empty;
    public string RiskTolerance { get; set; } = "Medium";
    public decimal MaxPositionSize { get; set; }
    public decimal StopLossPercent { get; set; }
    public decimal TakeProfitPercent { get; set; }
    public string TradingStyle { get; set; } = string.Empty;
    public string TradingObjective { get; set; } = string.Empty;
    public bool EnableNotifications { get; set; } = true;
    public string NotificationChannels { get; set; } = "Email";
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Customer Customer { get; set; } = null!;
}
