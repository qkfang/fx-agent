using System.Text.Json.Serialization;

namespace FxWebApi.Models;

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [JsonIgnore]
    public ICollection<CustomerPortfolio> Portfolios { get; set; } = new List<CustomerPortfolio>();
}

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

    [JsonIgnore]
    public Customer Customer { get; set; } = null!;
}

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

    [JsonIgnore]
    public Customer Customer { get; set; } = null!;
}

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
    public bool EnableNotifications { get; set; } = true;
    public string NotificationChannels { get; set; } = "Email";
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [JsonIgnore]
    public Customer Customer { get; set; } = null!;
}
