using FxWebPortal.Models;

namespace FxWebPortal.Services;

public class SuggestionService
{
    private readonly List<CustomerSuggestion> _suggestions;
    private int _nextId;

    public SuggestionService()
    {
        _suggestions = new List<CustomerSuggestion>
        {
            new CustomerSuggestion
            {
                Id = 1,
                CustomerName = "David Hartley",
                Phone = "+61 2 9000 1234",
                Email = "d.hartley@goldenstatesuper.com.au",
                Company = "Golden State Superannuation",
                CurrencyPair = "AUD/USD",
                Direction = "Buy",
                Analysis = "David manages a $450M commodity-linked equities fund. With the RBA signaling a rate pause and iron ore prices up 8% MoM, he is actively seeking AUD upside exposure. His fund's mandate allows 10% FX overlay, and he expressed interest in structured AUD/USD call options during last week's investor day.",
                Confidence = "High",
                SuggestedBy = "AI Market Intelligence",
                ReceivedAt = DateTime.UtcNow.AddMinutes(-42)
            },
            new CustomerSuggestion
            {
                Id = 2,
                CustomerName = "Priya Nair",
                Phone = "+61 3 8500 7788",
                Email = "priya.nair@meridiantrading.com.au",
                Company = "Meridian Trading Group",
                CurrencyPair = "AUD/USD",
                Direction = "Sell",
                Analysis = "Priya runs a macro desk at Meridian that has been short AUD since the China PMI miss last quarter. She is looking to add to existing AUD/USD short positions ahead of the next RBA decision. Our analysis indicates she read three AUD/USD bearish research notes this week (avg engagement 6 min). She recently inquired about shorting mechanisms via FX forwards.",
                Confidence = "High",
                SuggestedBy = "AI Market Intelligence",
                ReceivedAt = DateTime.UtcNow.AddMinutes(-110)
            },
            new CustomerSuggestion
            {
                Id = 3,
                CustomerName = "Marcus Webb",
                Phone = "+61 7 3200 5566",
                Email = "m.webb@pacificrimwealth.com",
                Company = "Pacific Rim Wealth Management",
                CurrencyPair = "AUD/USD",
                Direction = "Buy",
                Analysis = "Marcus manages HNW client portfolios with significant USD-denominated holdings. With the Fed indicating a dovish pivot and AUD/USD testing key support at 0.6450, he sees an attractive entry point for currency diversification. He attended our AUD/USD webinar last month and requested a follow-up call on execution strategies for spot and forward hedges.",
                Confidence = "Medium",
                SuggestedBy = "AI Market Intelligence",
                ReceivedAt = DateTime.UtcNow.AddHours(-3)
            }
        };
        _nextId = 4;
    }

    public List<CustomerSuggestion> GetAll() => _suggestions.OrderByDescending(s => s.ReceivedAt).ToList();

    public CustomerSuggestion? GetById(int id) => _suggestions.FirstOrDefault(s => s.Id == id);

    public CustomerSuggestion Add(CustomerSuggestion suggestion)
    {
        suggestion.Id = _nextId++;
        suggestion.ReceivedAt = DateTime.UtcNow;
        _suggestions.Add(suggestion);
        return suggestion;
    }
}
