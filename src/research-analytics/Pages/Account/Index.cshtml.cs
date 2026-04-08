using FxWebPortal.Models;
using FxWebPortal.Services;
using FxWebPortal.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using System.Text.Json;

namespace FxWebPortal.Pages.Account;

public class IndexModel : PageModel
{
    private readonly ArticleService _articles;
    private readonly SuggestionService _suggestions;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public IndexModel(ArticleService articles, SuggestionService suggestions,
        IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _articles = articles;
        _suggestions = suggestions;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public List<ResearchArticle> MarketInsights { get; set; } = new();
    public List<TraderRecommendation> Recommendations { get; set; } = new();
    public List<TraderSuggestion> TraderSuggestions { get; set; } = new();
    public string DisplayName { get; set; } = string.Empty;
    public List<string> Interests { get; set; } = new();
    public string Message { get; set; } = string.Empty;
    public string AuroraQuoteUrl { get; set; } = string.Empty;

    private IActionResult? RequireUser()
    {
        if (HttpContext.Session.GetString("UserAuth") != "true")
            return RedirectToPage("/Account/Login");
        return null;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var redirect = RequireUser();
        if (redirect != null) return redirect;
        await LoadDataAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostSendSuggestionToBroker(int id)
    {
        var redirect = RequireUser();
        if (redirect != null) return redirect;

        var suggestion = _suggestions.GetById(id);
        if (suggestion == null)
        {
            Message = "Suggestion not found.";
            await LoadDataAsync();
            return Page();
        }

        var brokerUrl = _configuration["CrmBrokerApi:EndpointUrl"];
        if (!string.IsNullOrWhiteSpace(brokerUrl))
        {
            try
            {
                var analysis = TextHelper.Truncate(suggestion.Analysis, 80);
                var payload = new
                {
                    userName = suggestion.CustomerName,
                    userEmail = suggestion.Email,
                    userCompany = suggestion.Company,
                    articleId = (int?)null,
                    articleTitle = analysis.Length > 0
                        ? $"AI Suggestion: {suggestion.Direction} {suggestion.CurrencyPair} — {analysis}"
                        : $"AI Suggestion: {suggestion.Direction} {suggestion.CurrencyPair}",
                    timeSpentSeconds = 0,
                    sessionId = $"suggestion-{suggestion.Id}"
                };
                var client = _httpClientFactory.CreateClient();
                var json = JsonSerializer.Serialize(payload);
                var response = await client.PostAsync(brokerUrl,
                    new StringContent(json, Encoding.UTF8, "application/json"));
                Message = response.IsSuccessStatusCode
                    ? $"Suggestion for {suggestion.CustomerName} pushed to broker successfully."
                    : $"Broker returned an error: {(int)response.StatusCode}.";
            }
            catch (Exception ex)
            {
                Message = $"Failed to reach broker CRM: {ex.Message}";
            }
        }
        else
        {
            Message = "Broker notification URL is not configured.";
        }

        await LoadDataAsync();
        return Page();
    }

    private async Task LoadDataAsync()
    {
        DisplayName = HttpContext.Session.GetString("UserDisplayName")
            ?? _configuration["User:DisplayName"] ?? "Trader";

        var interestsRaw = _configuration["User:Interests"] ?? string.Empty;
        Interests = interestsRaw
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();

        var tradingPlatformUrl = _configuration["TradingPlatformUrl"] ?? "http://localhost:5249";
        AuroraQuoteUrl = $"{tradingPlatformUrl}/api/quoto";

        // Market Insights: published articles filtered by user interests
        var allPublished = _articles.GetPublished();
        MarketInsights = Interests.Count > 0
            ? allPublished.Where(a => Interests.Contains(a.Category)).ToList()
            : allPublished;

        // Recommendations: load from api-intg filtered by logged-in trader
        var traderId = HttpContext.Session.GetInt32("TraderId") ?? 1;
        var apiBase = _configuration["IntegrationApi:BaseUrl"] ?? "http://localhost:5005";
        try
        {
            var client = _httpClientFactory.CreateClient();
            var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var recResponse = await client.GetAsync($"{apiBase}/api/traderrecommendations/trader/{traderId}");
            if (recResponse.IsSuccessStatusCode)
            {
                var json = await recResponse.Content.ReadAsStringAsync();
                Recommendations = System.Text.Json.JsonSerializer.Deserialize<List<TraderRecommendation>>(json, options) ?? new();
            }
        }
        catch
        {
            Recommendations = new();
        }

        // Customer Suggestions: load from api-intg filtered by logged-in trader
        try
        {
            var client = _httpClientFactory.CreateClient();
            var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var response = await client.GetAsync($"{apiBase}/api/tradersuggestions/trader/{traderId}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                TraderSuggestions = System.Text.Json.JsonSerializer.Deserialize<List<TraderSuggestion>>(json, options) ?? new();
            }
        }
        catch
        {
            TraderSuggestions = new();
        }
    }
}
