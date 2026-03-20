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
    public List<CustomerSuggestion> Suggestions { get; set; } = new();
    public string DisplayName { get; set; } = string.Empty;
    public List<string> Interests { get; set; } = new();
    public string Message { get; set; } = string.Empty;

    private IActionResult? RequireUser()
    {
        if (HttpContext.Session.GetString("UserAuth") != "true")
            return RedirectToPage("/Account/Login");
        return null;
    }

    public IActionResult OnGet()
    {
        var redirect = RequireUser();
        if (redirect != null) return redirect;
        LoadData();
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
            LoadData();
            return Page();
        }

        var brokerUrl = _configuration["BrokerNotification:EndpointUrl"];
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
                Message = $"Failed to reach broker backoffice: {ex.Message}";
            }
        }
        else
        {
            Message = "Broker notification URL is not configured.";
        }

        LoadData();
        return Page();
    }

    private void LoadData()
    {
        DisplayName = HttpContext.Session.GetString("UserDisplayName")
            ?? _configuration["User:DisplayName"] ?? "Trader";

        var interestsRaw = _configuration["User:Interests"] ?? string.Empty;
        Interests = interestsRaw
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();

        // Market Insights: published articles filtered by user interests
        var allPublished = _articles.GetPublished();
        MarketInsights = Interests.Count > 0
            ? allPublished.Where(a => Interests.Contains(a.Category)).ToList()
            : allPublished;

        // Suggest Customer: suggestions matching user's preferred currency pairs
        var allSuggestions = _suggestions.GetAll();
        Suggestions = Interests.Count > 0
            ? allSuggestions.Where(s => Interests.Contains(s.CurrencyPair)).ToList()
            : allSuggestions;
    }
}
