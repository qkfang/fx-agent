using Microsoft.AspNetCore.Mvc;
using FxIntegrationApi.Data;
using FxIntegrationApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace FxIntegrationApi.Controllers;

[ApiController]
[Route("mcp")]
public class McpController : ControllerBase
{
    private readonly FxDbContext _db;
    private readonly ILogger<McpController> _logger;

    public McpController(FxDbContext db, ILogger<McpController> logger)
    {
        _db = db;
        _logger = logger;
    }

    [HttpPost("call")]
    public async Task<ActionResult> Call([FromBody] McpToolCall request)
    {
        _logger.LogInformation("MCP call: {Tool}", request.Tool);

        try
        {
            var result = request.Tool switch
            {
                "get_all_customers" => await GetAllCustomers(),
                "get_customer" => await GetCustomer(GetInt(request, "id")),
                "create_customer" => await CreateCustomer(request),
                "update_customer" => await UpdateCustomer(request),
                "delete_customer" => await DeleteCustomer(GetInt(request, "id")),

                "get_customer_portfolios" => await GetCustomerPortfolios(GetInt(request, "customerId")),
                "get_portfolio" => await GetPortfolio(GetInt(request, "id")),
                "create_portfolio" => await CreatePortfolio(request),
                "update_portfolio" => await UpdatePortfolio(request),
                "delete_portfolio" => await DeletePortfolio(GetInt(request, "id")),

                "get_all_traders" => await GetAllTraders(),
                "get_trader" => await GetTrader(GetInt(request, "id")),
                "create_trader" => await CreateTrader(request),

                "get_all_research_articles" => await GetAllResearchArticles(),
                "get_research_article" => await GetResearchArticle(GetInt(request, "id")),
                "create_research_article" => await CreateResearchArticle(request),

                "get_customer_preferences" => await GetCustomerPreferences(GetInt(request, "customerId")),
                "update_customer_preferences" => await UpdateCustomerPreferences(request),

                "get_customer_history" => await GetCustomerHistory(GetInt(request, "customerId")),

                "get_all_research_drafts" => await GetAllResearchDrafts(),
                "create_research_draft" => await CreateResearchDraft(request),

                "get_all_research_patterns" => await GetAllResearchPatterns(),
                "create_research_pattern" => await CreateResearchPattern(request),

                "get_trader_news" => await GetTraderNews(GetInt(request, "traderId")),
                "get_trader_recommendations" => await GetTraderRecommendations(GetInt(request, "traderId")),

                _ => McpError($"Unknown tool: {request.Tool}")
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MCP tool execution failed: {Tool}", request.Tool);
            return StatusCode(500, McpError(ex.Message));
        }
    }

    // Customer tools
    private async Task<object> GetAllCustomers()
    {
        var customers = await _db.Customers.Include(c => c.Portfolios).ToListAsync();
        return McpOk(customers);
    }

    private async Task<object> GetCustomer(int id)
    {
        var customer = await _db.Customers.Include(c => c.Portfolios).FirstOrDefaultAsync(c => c.Id == id);
        return customer is null ? McpError("Customer not found") : McpOk(customer);
    }

    private async Task<object> CreateCustomer(McpToolCall request)
    {
        var customer = new Customer
        {
            Name = GetString(request, "name")!,
            Email = GetString(request, "email")!,
            Phone = GetString(request, "phone")!,
            Company = GetString(request, "company")!
        };
        _db.Customers.Add(customer);
        await _db.SaveChangesAsync();
        return McpOk(customer);
    }

    private async Task<object> UpdateCustomer(McpToolCall request)
    {
        var id = GetInt(request, "id");
        var customer = await _db.Customers.FindAsync(id);
        if (customer is null) return McpError("Customer not found");

        customer.Name = GetString(request, "name")!;
        customer.Email = GetString(request, "email")!;
        customer.Phone = GetString(request, "phone")!;
        customer.Company = GetString(request, "company")!;
        await _db.SaveChangesAsync();
        return McpOk("Updated successfully");
    }

    private async Task<object> DeleteCustomer(int id)
    {
        var customer = await _db.Customers.FindAsync(id);
        if (customer is null) return McpError("Customer not found");
        _db.Customers.Remove(customer);
        await _db.SaveChangesAsync();
        return McpOk("Deleted successfully");
    }

    // Portfolio tools
    private async Task<object> GetCustomerPortfolios(int customerId)
    {
        var portfolios = await _db.CustomerPortfolios.Where(p => p.CustomerId == customerId).ToListAsync();
        return McpOk(portfolios);
    }

    private async Task<object> GetPortfolio(int id)
    {
        var portfolio = await _db.CustomerPortfolios.FindAsync(id);
        return portfolio is null ? McpError("Portfolio not found") : McpOk(portfolio);
    }

    private async Task<object> CreatePortfolio(McpToolCall request)
    {
        var portfolio = new CustomerPortfolio
        {
            CustomerId = GetInt(request, "customerId"),
            CurrencyPair = GetString(request, "currencyPair")!,
            Direction = GetString(request, "direction")!,
            Amount = GetDecimal(request, "amount"),
            EntryRate = GetDecimal(request, "entryRate"),
            Status = GetString(request, "status") ?? "Open"
        };
        _db.CustomerPortfolios.Add(portfolio);
        await _db.SaveChangesAsync();
        return McpOk(portfolio);
    }

    private async Task<object> UpdatePortfolio(McpToolCall request)
    {
        var id = GetInt(request, "id");
        var portfolio = await _db.CustomerPortfolios.FindAsync(id);
        if (portfolio is null) return McpError("Portfolio not found");

        portfolio.CustomerId = GetInt(request, "customerId");
        portfolio.CurrencyPair = GetString(request, "currencyPair")!;
        portfolio.Direction = GetString(request, "direction")!;
        portfolio.Amount = GetDecimal(request, "amount");
        portfolio.EntryRate = GetDecimal(request, "entryRate");
        portfolio.Status = GetString(request, "status")!;
        await _db.SaveChangesAsync();
        return McpOk("Updated successfully");
    }

    private async Task<object> DeletePortfolio(int id)
    {
        var portfolio = await _db.CustomerPortfolios.FindAsync(id);
        if (portfolio is null) return McpError("Portfolio not found");
        _db.CustomerPortfolios.Remove(portfolio);
        await _db.SaveChangesAsync();
        return McpOk("Deleted successfully");
    }

    // Trader tools
    private async Task<object> GetAllTraders()
    {
        var traders = await _db.Traders.ToListAsync();
        return McpOk(traders);
    }

    private async Task<object> GetTrader(int id)
    {
        var trader = await _db.Traders.FindAsync(id);
        return trader is null ? McpError("Trader not found") : McpOk(trader);
    }

    private async Task<object> CreateTrader(McpToolCall request)
    {
        var trader = new Trader
        {
            Name = GetString(request, "name")!,
            Email = GetString(request, "email")!,
            Specialization = GetString(request, "expertise") ?? string.Empty,
            JoinedAt = DateTime.UtcNow
        };
        _db.Traders.Add(trader);
        await _db.SaveChangesAsync();
        return McpOk(trader);
    }

    // Research Article tools
    private async Task<object> GetAllResearchArticles()
    {
        var articles = await _db.ResearchArticles.ToListAsync();
        return McpOk(articles);
    }

    private async Task<object> GetResearchArticle(int id)
    {
        var article = await _db.ResearchArticles.FindAsync(id);
        return article is null ? McpError("Article not found") : McpOk(article);
    }

    private async Task<object> CreateResearchArticle(McpToolCall request)
    {
        var article = new ResearchArticle
        {
            Title = GetString(request, "title")!,
            Content = GetString(request, "content")!,
            Category = GetString(request, "currencyPair") ?? string.Empty,
            Summary = GetString(request, "analysis") ?? string.Empty,
            PublishedDate = DateTime.UtcNow
        };
        _db.ResearchArticles.Add(article);
        await _db.SaveChangesAsync();
        return McpOk(article);
    }

    // Customer Preferences tools
    private async Task<object> GetCustomerPreferences(int customerId)
    {
        var prefs = await _db.CustomerPreferences.Where(p => p.CustomerId == customerId).ToListAsync();
        return McpOk(prefs);
    }

    private async Task<object> UpdateCustomerPreferences(McpToolCall request)
    {
        var id = GetInt(request, "id");
        var pref = await _db.CustomerPreferences.FindAsync(id);
        if (pref is null) return McpError("Preferences not found");

        pref.CustomerId = GetInt(request, "customerId");
        pref.RiskTolerance = GetString(request, "riskLevel") ?? pref.RiskTolerance;
        pref.PreferredCurrencyPairs = GetString(request, "preferredPairs") ?? pref.PreferredCurrencyPairs;
        pref.TradingObjective = GetString(request, "tradingObjective") ?? pref.TradingObjective;
        pref.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return McpOk("Updated successfully");
    }

    // Customer History tools
    private async Task<object> GetCustomerHistory(int customerId)
    {
        var history = await _db.CustomerHistories.Where(h => h.CustomerId == customerId).ToListAsync();
        return McpOk(history);
    }

    // Research Draft tools
    private async Task<object> GetAllResearchDrafts()
    {
        var drafts = await _db.ResearchDrafts.ToListAsync();
        return McpOk(drafts);
    }

    private async Task<object> CreateResearchDraft(McpToolCall request)
    {
        var draft = new ResearchDraft
        {
            Title = GetString(request, "title")!,
            Content = GetString(request, "content")!,
            Status = GetString(request, "status")!
        };
        _db.ResearchDrafts.Add(draft);
        await _db.SaveChangesAsync();
        return McpOk(draft);
    }

    // Research Pattern tools
    private async Task<object> GetAllResearchPatterns()
    {
        var patterns = await _db.ResearchPatterns.ToListAsync();
        return McpOk(patterns);
    }

    private async Task<object> CreateResearchPattern(McpToolCall request)
    {
        var pattern = new ResearchPattern
        {
            PatternName = GetString(request, "patternName")!,
            Description = GetString(request, "description")!,
            CurrencyPair = GetString(request, "currencyPair")!
        };
        _db.ResearchPatterns.Add(pattern);
        await _db.SaveChangesAsync();
        return McpOk(pattern);
    }

    // Trader News Feed tools
    private async Task<object> GetTraderNews(int traderId)
    {
        var news = await _db.TraderNewsFeeds.Where(n => n.TraderId == traderId).ToListAsync();
        return McpOk(news);
    }

    // Trader Recommendations tools
    private async Task<object> GetTraderRecommendations(int traderId)
    {
        var recs = await _db.TraderRecommendations.Where(r => r.TraderId == traderId).ToListAsync();
        return McpOk(recs);
    }

    // Helper methods
    private static object McpOk(object data) =>
        new { success = true, message = "OK", data };

    private static object McpError(string message) =>
        new { success = false, message, data = (object?)null };

    private static int GetInt(McpToolCall call, string key, int defaultValue = 0)
    {
        if (call.Parameters.TryGetValue(key, out var val) && val is JsonElement elem && elem.ValueKind == JsonValueKind.Number)
            return elem.GetInt32();
        return defaultValue;
    }

    private static decimal GetDecimal(McpToolCall call, string key, decimal defaultValue = 0)
    {
        if (call.Parameters.TryGetValue(key, out var val) && val is JsonElement elem && elem.ValueKind == JsonValueKind.Number)
            return elem.GetDecimal();
        return defaultValue;
    }

    private static string? GetString(McpToolCall call, string key)
    {
        if (call.Parameters.TryGetValue(key, out var val) && val is JsonElement elem && elem.ValueKind == JsonValueKind.String)
            return elem.GetString();
        return null;
    }
}

public class McpToolCall
{
    public string Tool { get; set; } = string.Empty;
    public Dictionary<string, object> Parameters { get; set; } = new();
}
