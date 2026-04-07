using FxIntegrationApi.Data;
using FxIntegrationApi.Models;
using Microsoft.EntityFrameworkCore;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;

namespace FxIntegrationApi.Mcp;

[McpServerToolType]
public class FxMcpTools(FxDbContext db, ILogger<FxMcpTools> logger)
{
    private static readonly JsonSerializerOptions _jsonOptions = new() 
    { 
        ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles 
    };

    [McpServerTool(Name = "get_all_customers"), Description("Get all customers with their portfolios")]
    public async Task<string> GetAllCustomers()
    {
        logger.LogTrace("MCP tool called: get_all_customers");
        var customers = await db.Customers.Include(c => c.Portfolios).ToListAsync();
        return JsonSerializer.Serialize(customers, _jsonOptions);
    }

    [McpServerTool(Name = "get_customer"), Description("Get customer by ID")]
    public async Task<string> GetCustomer([Description("Customer ID")] int id)
    {
        logger.LogTrace("MCP tool called: get_customer, id={Id}", id);
        var customer = await db.Customers.Include(c => c.Portfolios).FirstOrDefaultAsync(c => c.Id == id);
        return customer is null ? JsonSerializer.Serialize(new { error = "Not found" }) : JsonSerializer.Serialize(customer, _jsonOptions);
    }

    [McpServerTool(Name = "create_customer"), Description("Create new customer")]
    public async Task<string> CreateCustomer(
        [Description("Customer name")] string name,
        [Description("Email address")] string email,
        [Description("Phone number")] string phone,
        [Description("Company name")] string company)
    {
        logger.LogTrace("MCP tool called: create_customer, name={Name}", name);
        var customer = new Customer { Name = name, Email = email, Phone = phone, Company = company };
        db.Customers.Add(customer);
        await db.SaveChangesAsync();
        return JsonSerializer.Serialize(customer, _jsonOptions);
    }

    [McpServerTool(Name = "update_customer"), Description("Update customer information")]
    public async Task<string> UpdateCustomer(
        [Description("Customer ID")] int id,
        [Description("Customer name")] string name,
        [Description("Email address")] string email,
        [Description("Phone number")] string phone,
        [Description("Company name")] string company)
    {
        logger.LogTrace("MCP tool called: update_customer, id={Id}", id);
        var customer = await db.Customers.FindAsync(id);
        if (customer is null) return JsonSerializer.Serialize(new { error = "Not found" });
        customer.Name = name;
        customer.Email = email;
        customer.Phone = phone;
        customer.Company = company;
        await db.SaveChangesAsync();
        return JsonSerializer.Serialize(new { success = true });
    }

    [McpServerTool(Name = "delete_customer"), Description("Delete customer")]
    public async Task<string> DeleteCustomer([Description("Customer ID")] int id)
    {
        logger.LogTrace("MCP tool called: delete_customer, id={Id}", id);
        var customer = await db.Customers.FindAsync(id);
        if (customer is null) return JsonSerializer.Serialize(new { error = "Not found" });
        db.Customers.Remove(customer);
        await db.SaveChangesAsync();
        return JsonSerializer.Serialize(new { success = true });
    }

    [McpServerTool(Name = "get_customer_portfolios"), Description("Get all portfolios for a customer")]
    public async Task<string> GetCustomerPortfolios([Description("Customer ID")] int customerId)
    {
        logger.LogTrace("MCP tool called: get_customer_portfolios, customerId={CustomerId}", customerId);
        var portfolios = await db.CustomerPortfolios.Where(p => p.CustomerId == customerId).ToListAsync();
        return JsonSerializer.Serialize(portfolios, _jsonOptions);
    }

    [McpServerTool(Name = "get_portfolio"), Description("Get portfolio by ID")]
    public async Task<string> GetPortfolio([Description("Portfolio ID")] int id)
    {
        logger.LogTrace("MCP tool called: get_portfolio, id={Id}", id);
        var portfolio = await db.CustomerPortfolios.FindAsync(id);
        return portfolio is null ? JsonSerializer.Serialize(new { error = "Not found" }) : JsonSerializer.Serialize(portfolio, _jsonOptions);
    }

    [McpServerTool(Name = "create_portfolio"), Description("Create new portfolio position")]
    public async Task<string> CreatePortfolio(
        [Description("Customer ID")] int customerId,
        [Description("Currency pair, e.g. EUR/USD")] string currencyPair,
        [Description("Direction: Buy or Sell")] string direction,
        [Description("Position amount")] decimal amount,
        [Description("Entry rate")] decimal entryRate,
        [Description("Position status")] string status = "Open")
    {
        logger.LogTrace("MCP tool called: create_portfolio, customerId={CustomerId}, currencyPair={CurrencyPair}", customerId, currencyPair);
        var portfolio = new CustomerPortfolio { CustomerId = customerId, CurrencyPair = currencyPair, Direction = direction, Amount = amount, EntryRate = entryRate, Status = status };
        db.CustomerPortfolios.Add(portfolio);
        await db.SaveChangesAsync();
        return JsonSerializer.Serialize(portfolio, _jsonOptions);
    }

    [McpServerTool(Name = "update_portfolio"), Description("Update portfolio position")]
    public async Task<string> UpdatePortfolio(
        [Description("Portfolio ID")] int id,
        [Description("Customer ID")] int customerId,
        [Description("Currency pair")] string currencyPair,
        [Description("Direction: Buy or Sell")] string direction,
        [Description("Position amount")] decimal amount,
        [Description("Entry rate")] decimal entryRate,
        [Description("Position status")] string status)
    {
        logger.LogTrace("MCP tool called: update_portfolio, id={Id}", id);
        var portfolio = await db.CustomerPortfolios.FindAsync(id);
        if (portfolio is null) return JsonSerializer.Serialize(new { error = "Not found" });
        portfolio.CustomerId = customerId;
        portfolio.CurrencyPair = currencyPair;
        portfolio.Direction = direction;
        portfolio.Amount = amount;
        portfolio.EntryRate = entryRate;
        portfolio.Status = status;
        await db.SaveChangesAsync();
        return JsonSerializer.Serialize(new { success = true });
    }

    [McpServerTool(Name = "delete_portfolio"), Description("Delete portfolio position")]
    public async Task<string> DeletePortfolio([Description("Portfolio ID")] int id)
    {
        logger.LogTrace("MCP tool called: delete_portfolio, id={Id}", id);
        var portfolio = await db.CustomerPortfolios.FindAsync(id);
        if (portfolio is null) return JsonSerializer.Serialize(new { error = "Not found" });
        db.CustomerPortfolios.Remove(portfolio);
        await db.SaveChangesAsync();
        return JsonSerializer.Serialize(new { success = true });
    }

    [McpServerTool(Name = "get_all_traders"), Description("Get all traders with recommendations and feeds")]
    public async Task<string> GetAllTraders()
    {
        logger.LogTrace("MCP tool called: get_all_traders");
        var traders = await db.Traders.Include(t => t.Recommendations).Include(t => t.NewsFeeds).ToListAsync();
        return JsonSerializer.Serialize(traders, _jsonOptions);
    }

    [McpServerTool(Name = "get_trader"), Description("Get trader by ID")]
    public async Task<string> GetTrader([Description("Trader ID")] int id)
    {
        logger.LogTrace("MCP tool called: get_trader, id={Id}", id);
        var trader = await db.Traders.Include(t => t.Recommendations).Include(t => t.NewsFeeds).FirstOrDefaultAsync(t => t.Id == id);
        return trader is null ? JsonSerializer.Serialize(new { error = "Not found" }) : JsonSerializer.Serialize(trader, _jsonOptions);
    }

    [McpServerTool(Name = "create_trader"), Description("Create new trader")]
    public async Task<string> CreateTrader(
        [Description("Trader name")] string name,
        [Description("Email address")] string email,
        [Description("Trading desk")] string desk,
        [Description("Area of specialization")] string specialization,
        [Description("Trading region")] string region)
    {
        logger.LogTrace("MCP tool called: create_trader, name={Name}", name);
        var trader = new Trader { Name = name, Email = email, Desk = desk, Specialization = specialization, Region = region, JoinedAt = DateTime.UtcNow };
        db.Traders.Add(trader);
        await db.SaveChangesAsync();
        return JsonSerializer.Serialize(trader, _jsonOptions);
    }

    [McpServerTool(Name = "get_all_research_articles"), Description("Get all research articles")]
    public async Task<string> GetAllResearchArticles()
    {
        logger.LogTrace("MCP tool called: get_all_research_articles");
        var articles = await db.ResearchArticles.ToListAsync();
        return JsonSerializer.Serialize(articles, _jsonOptions);
    }

    [McpServerTool(Name = "get_research_article"), Description("Get research article by ID")]
    public async Task<string> GetResearchArticle([Description("Article ID")] int id)
    {
        logger.LogTrace("MCP tool called: get_research_article, id={Id}", id);
        var article = await db.ResearchArticles.FindAsync(id);
        return article is null ? JsonSerializer.Serialize(new { error = "Not found" }) : JsonSerializer.Serialize(article, _jsonOptions);
    }

    [McpServerTool(Name = "create_research_article"), Description("Create new research article")]
    public async Task<string> CreateResearchArticle(
        [Description("Article title")] string title,
        [Description("Article summary")] string summary,
        [Description("Full article content")] string content,
        [Description("Category")] string category,
        [Description("Author name")] string author,
        [Description("Sentiment: Bullish, Bearish, or Neutral")] string sentiment = "Neutral")
    {
        logger.LogTrace("MCP tool called: create_research_article, title={Title}", title);
        var article = new ResearchArticle { Title = title, Summary = summary, Content = content, Category = category, Author = author, Sentiment = sentiment, PublishedDate = DateTime.UtcNow };
        db.ResearchArticles.Add(article);
        await db.SaveChangesAsync();
        return JsonSerializer.Serialize(article, _jsonOptions);
    }

    [McpServerTool(Name = "get_customer_preferences"), Description("Get customer trading preferences")]
    public async Task<string> GetCustomerPreferences([Description("Customer ID")] int customerId)
    {
        logger.LogTrace("MCP tool called: get_customer_preferences, customerId={CustomerId}", customerId);
        var pref = await db.CustomerPreferences.FirstOrDefaultAsync(p => p.CustomerId == customerId);
        return pref is null ? JsonSerializer.Serialize(new { error = "Not found" }) : JsonSerializer.Serialize(pref, _jsonOptions);
    }

    [McpServerTool(Name = "update_customer_preferences"), Description("Update customer trading preferences")]
    public async Task<string> UpdateCustomerPreferences(
        [Description("Preference record ID")] int id,
        [Description("Customer ID")] int customerId,
        [Description("Preferred currency pairs")] string preferredCurrencyPairs,
        [Description("Risk tolerance level")] string riskTolerance,
        [Description("Trading style")] string tradingStyle,
        [Description("Trading objective")] string tradingObjective)
    {
        logger.LogTrace("MCP tool called: update_customer_preferences, id={Id}, customerId={CustomerId}", id, customerId);
        var pref = await db.CustomerPreferences.FindAsync(id);
        if (pref is null) return JsonSerializer.Serialize(new { error = "Not found" });
        pref.CustomerId = customerId;
        pref.PreferredCurrencyPairs = preferredCurrencyPairs;
        pref.RiskTolerance = riskTolerance;
        pref.TradingStyle = tradingStyle;
        pref.TradingObjective = tradingObjective;
        pref.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return JsonSerializer.Serialize(new { success = true });
    }

    [McpServerTool(Name = "get_customer_history"), Description("Get customer trading history")]
    public async Task<string> GetCustomerHistory([Description("Customer ID")] int customerId)
    {
        logger.LogTrace("MCP tool called: get_customer_history, customerId={CustomerId}", customerId);
        var history = await db.CustomerHistories.Where(h => h.CustomerId == customerId).ToListAsync();
        return JsonSerializer.Serialize(history, _jsonOptions);
    }

    [McpServerTool(Name = "get_all_research_drafts"), Description("Get all research drafts")]
    public async Task<string> GetAllResearchDrafts()
    {
        logger.LogTrace("MCP tool called: get_all_research_drafts");
        var drafts = await db.ResearchDrafts.ToListAsync();
        return JsonSerializer.Serialize(drafts, _jsonOptions);
    }

    [McpServerTool(Name = "create_research_draft"), Description("Create new research draft")]
    public async Task<string> CreateResearchDraft(
        [Description("Draft title")] string title,
        [Description("Draft content")] string content,
        [Description("Author name")] string author,
        [Description("Category")] string category,
        [Description("Status")] string status = "InProgress")
    {
        logger.LogTrace("MCP tool called: create_research_draft, title={Title}", title);
        var draft = new ResearchDraft { Title = title, Content = content, Author = author, Category = category, Status = status };
        db.ResearchDrafts.Add(draft);
        await db.SaveChangesAsync();
        return JsonSerializer.Serialize(draft, _jsonOptions);
    }

    [McpServerTool(Name = "get_all_research_patterns"), Description("Get all identified trading patterns")]
    public async Task<string> GetAllResearchPatterns()
    {
        logger.LogTrace("MCP tool called: get_all_research_patterns");
        var patterns = await db.ResearchPatterns.ToListAsync();
        return JsonSerializer.Serialize(patterns, _jsonOptions);
    }

    [McpServerTool(Name = "create_research_pattern"), Description("Create new pattern observation")]
    public async Task<string> CreateResearchPattern(
        [Description("Currency pair")] string currencyPair,
        [Description("Pattern name")] string patternName,
        [Description("Timeframe")] string timeframe,
        [Description("Direction: Bullish or Bearish")] string direction,
        [Description("Pattern description")] string description,
        [Description("Detected by")] string detectedBy)
    {
        logger.LogTrace("MCP tool called: create_research_pattern, currencyPair={CurrencyPair}, patternName={PatternName}", currencyPair, patternName);
        var pattern = new ResearchPattern { CurrencyPair = currencyPair, PatternName = patternName, Timeframe = timeframe, Direction = direction, Description = description, DetectedBy = detectedBy, DetectedAt = DateTime.UtcNow };
        db.ResearchPatterns.Add(pattern);
        await db.SaveChangesAsync();
        return JsonSerializer.Serialize(pattern, _jsonOptions);
    }

    [McpServerTool(Name = "get_trader_news"), Description("Get news feeds for a trader")]
    public async Task<string> GetTraderNews([Description("Trader ID")] int traderId)
    {
        logger.LogTrace("MCP tool called: get_trader_news, traderId={TraderId}", traderId);
        var news = await db.TraderNewsFeeds.Where(n => n.TraderId == traderId).ToListAsync();
        return JsonSerializer.Serialize(news, _jsonOptions);
    }

    [McpServerTool(Name = "get_trader_recommendations"), Description("Get trader recommendations")]
    public async Task<string> GetTraderRecommendations([Description("Trader ID")] int traderId)
    {
        logger.LogTrace("MCP tool called: get_trader_recommendations, traderId={TraderId}", traderId);
        var recs = await db.TraderRecommendations.Where(r => r.TraderId == traderId).ToListAsync();
        return JsonSerializer.Serialize(recs, _jsonOptions);
    }

    [McpServerTool(Name = "get_all_trader_suggestions"), Description("Get all trader suggestions matching research articles with customers")]
    public async Task<string> GetAllTraderSuggestions()
    {
        logger.LogTrace("MCP tool called: get_all_trader_suggestions");
        var suggestions = await db.TraderSuggestions
            .Include(s => s.Trader)
            .Include(s => s.Customer)
            .Include(s => s.ResearchArticle)
            .ToListAsync();
        return JsonSerializer.Serialize(suggestions, _jsonOptions);
    }

    [McpServerTool(Name = "get_trader_suggestions"), Description("Get suggestions created by a specific trader")]
    public async Task<string> GetTraderSuggestions([Description("Trader ID")] int traderId)
    {
        logger.LogTrace("MCP tool called: get_trader_suggestions, traderId={TraderId}", traderId);
        var suggestions = await db.TraderSuggestions
            .Include(s => s.Customer)
            .Include(s => s.ResearchArticle)
            .Where(s => s.TraderId == traderId)
            .ToListAsync();
        return JsonSerializer.Serialize(suggestions, _jsonOptions);
    }

    [McpServerTool(Name = "get_customer_suggestions"), Description("Get research article suggestions for a specific customer")]
    public async Task<string> GetCustomerSuggestions([Description("Customer ID")] int customerId)
    {
        logger.LogTrace("MCP tool called: get_customer_suggestions, customerId={CustomerId}", customerId);
        var suggestions = await db.TraderSuggestions
            .Include(s => s.Trader)
            .Include(s => s.ResearchArticle)
            .Where(s => s.CustomerId == customerId)
            .ToListAsync();
        return JsonSerializer.Serialize(suggestions, _jsonOptions);
    }

    [McpServerTool(Name = "create_trader_suggestion"), Description("Create a new trader suggestion matching a research article with a customer")]
    public async Task<string> CreateTraderSuggestion(
        [Description("Trader ID creating the suggestion")] int traderId,
        [Description("Customer ID to receive the suggestion")] int customerId,
        [Description("Research article ID being suggested")] int researchArticleId,
        [Description("Reasoning explaining why this article is relevant for this customer")] string reasoning,
        [Description("Relevance score: High, Medium, or Low")] string relevanceScore = "Medium")
    {
        logger.LogTrace("MCP tool called: create_trader_suggestion, traderId={TraderId}, customerId={CustomerId}, articleId={ArticleId}", traderId, customerId, researchArticleId);
        var suggestion = new TraderSuggestion
        {
            TraderId = traderId,
            CustomerId = customerId,
            ResearchArticleId = researchArticleId,
            Reasoning = reasoning,
            RelevanceScore = relevanceScore,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };
        db.TraderSuggestions.Add(suggestion);
        await db.SaveChangesAsync();
        return JsonSerializer.Serialize(suggestion, _jsonOptions);
    }

    [McpServerTool(Name = "update_trader_suggestion_status"), Description("Update the status of a trader suggestion (Pending, Sent, Dismissed)")]
    public async Task<string> UpdateTraderSuggestionStatus(
        [Description("Suggestion ID")] int id,
        [Description("New status: Pending, Sent, or Dismissed")] string status)
    {
        logger.LogTrace("MCP tool called: update_trader_suggestion_status, id={Id}, status={Status}", id, status);
        var suggestion = await db.TraderSuggestions.FindAsync(id);
        if (suggestion is null) return JsonSerializer.Serialize(new { error = "Not found" });
        suggestion.Status = status;
        await db.SaveChangesAsync();
        return JsonSerializer.Serialize(new { success = true });
    }

    [McpServerTool(Name = "delete_trader_suggestion"), Description("Delete a trader suggestion")]
    public async Task<string> DeleteTraderSuggestion([Description("Suggestion ID")] int id)
    {
        logger.LogTrace("MCP tool called: delete_trader_suggestion, id={Id}", id);
        var suggestion = await db.TraderSuggestions.FindAsync(id);
        if (suggestion is null) return JsonSerializer.Serialize(new { error = "Not found" });
        db.TraderSuggestions.Remove(suggestion);
        await db.SaveChangesAsync();
        return JsonSerializer.Serialize(new { success = true });
    }
}
