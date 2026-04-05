using FxWebPortal.Models;
using System.Text.Json;

namespace FxWebPortal.Services;

public class SuggestionService
{
    private readonly HttpClient _http;
    private readonly string _baseUrl;
    private readonly ILogger<SuggestionService> _logger;
    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public SuggestionService(HttpClient http, IConfiguration config, ILogger<SuggestionService> logger)
    {
        _http = http;
        _baseUrl = config["IntegrationApi:BaseUrl"]?.TrimEnd('/') ?? "http://localhost:5005";
        _logger = logger;
    }

    public List<CustomerSuggestion> GetAll()
    {
        try
        {
            var recommendations = FetchRecommendations();
            var customers = FetchCustomers();

            var suggestions = new List<CustomerSuggestion>();
            int id = 1;
            foreach (var rec in recommendations)
            {
                var matched = customers
                    .Where(c => c.Portfolios.Any(p => p.CurrencyPair == rec.CurrencyPair))
                    .ToList();

                foreach (var customer in matched)
                {
                    suggestions.Add(new CustomerSuggestion
                    {
                        Id = id++,
                        CustomerName = customer.Name,
                        Email = customer.Email,
                        Phone = customer.Phone,
                        Company = customer.Company,
                        CurrencyPair = rec.CurrencyPair,
                        Direction = rec.Direction,
                        Analysis = rec.Rationale,
                        Confidence = rec.Confidence,
                        SuggestedBy = rec.Trader?.Name ?? "AI",
                        ReceivedAt = rec.CreatedAt
                    });
                }
            }

            return suggestions.OrderByDescending(s => s.ReceivedAt).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to build suggestions from integration API");
            return new();
        }
    }

    public CustomerSuggestion? GetById(int id)
    {
        return GetAll().FirstOrDefault(s => s.Id == id);
    }

    private List<TraderRecommendation> FetchRecommendations()
    {
        var response = _http.GetAsync($"{_baseUrl}/api/traderrecommendations").Result;
        response.EnsureSuccessStatusCode();
        var json = response.Content.ReadAsStringAsync().Result;
        return JsonSerializer.Deserialize<List<TraderRecommendation>>(json, _jsonOptions) ?? new();
    }

    private List<Customer> FetchCustomers()
    {
        var response = _http.GetAsync($"{_baseUrl}/api/customers").Result;
        response.EnsureSuccessStatusCode();
        var json = response.Content.ReadAsStringAsync().Result;
        return JsonSerializer.Deserialize<List<Customer>>(json, _jsonOptions) ?? new();
    }
}
