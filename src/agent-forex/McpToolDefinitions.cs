using Azure.AI.Projects.Agents;
using System.ComponentModel;
using System.Reflection;
using System.Text.Json;

namespace FxAgent;

public static class McpToolDefinitions
{
    public static IEnumerable<ToolDefinition> GetAllToolDefinitions()
    {
        yield return new FunctionToolDefinition("get_all_customers", "Get all customers with their portfolios");
        yield return new FunctionToolDefinition("get_customer", "Get customer by ID", new BinaryData(JsonSerializer.Serialize(new
        {
            type = "object",
            properties = new { id = new { type = "integer", description = "Customer ID" } },
            required = new[] { "id" }
        })));
        yield return new FunctionToolDefinition("create_customer", "Create new customer", new BinaryData(JsonSerializer.Serialize(new
        {
            type = "object",
            properties = new
            {
                name = new { type = "string", description = "Customer name" },
                email = new { type = "string", description = "Email address" },
                phone = new { type = "string", description = "Phone number" },
                company = new { type = "string", description = "Company name" }
            },
            required = new[] { "name", "email", "phone", "company" }
        })));
        yield return new FunctionToolDefinition("update_customer", "Update customer information", new BinaryData(JsonSerializer.Serialize(new
        {
            type = "object",
            properties = new
            {
                id = new { type = "integer", description = "Customer ID" },
                name = new { type = "string", description = "Customer name" },
                email = new { type = "string", description = "Email address" },
                phone = new { type = "string", description = "Phone number" },
                company = new { type = "string", description = "Company name" }
            },
            required = new[] { "id", "name", "email", "phone", "company" }
        })));
        yield return new FunctionToolDefinition("delete_customer", "Delete customer", new BinaryData(JsonSerializer.Serialize(new
        {
            type = "object",
            properties = new { id = new { type = "integer", description = "Customer ID" } },
            required = new[] { "id" }
        })));

        yield return new FunctionToolDefinition("get_customer_portfolios", "Get portfolios for a customer", new BinaryData(JsonSerializer.Serialize(new
        {
            type = "object",
            properties = new { customerId = new { type = "integer", description = "Customer ID" } },
            required = new[] { "customerId" }
        })));
        yield return new FunctionToolDefinition("get_portfolio", "Get portfolio by ID", new BinaryData(JsonSerializer.Serialize(new
        {
            type = "object",
            properties = new { id = new { type = "integer", description = "Portfolio ID" } },
            required = new[] { "id" }
        })));
        yield return new FunctionToolDefinition("create_portfolio", "Create new portfolio position", new BinaryData(JsonSerializer.Serialize(new
        {
            type = "object",
            properties = new
            {
                customerId = new { type = "integer", description = "Customer ID" },
                currencyPair = new { type = "string", description = "Currency pair (e.g., EUR/USD)" },
                direction = new { type = "string", description = "Trade direction (Buy/Sell)" },
                amount = new { type = "number", description = "Trade amount" },
                entryRate = new { type = "number", description = "Entry exchange rate" },
                status = new { type = "string", description = "Position status (Open/Closed)" }
            },
            required = new[] { "customerId", "currencyPair", "direction", "amount", "entryRate" }
        })));
        yield return new FunctionToolDefinition("update_portfolio", "Update portfolio position", new BinaryData(JsonSerializer.Serialize(new
        {
            type = "object",
            properties = new
            {
                id = new { type = "integer", description = "Portfolio ID" },
                customerId = new { type = "integer", description = "Customer ID" },
                currencyPair = new { type = "string", description = "Currency pair" },
                direction = new { type = "string", description = "Direction" },
                amount = new { type = "number", description = "Amount" },
                entryRate = new { type = "number", description = "Entry rate" },
                status = new { type = "string", description = "Status" }
            },
            required = new[] { "id", "customerId", "currencyPair", "direction", "amount", "entryRate", "status" }
        })));
        yield return new FunctionToolDefinition("delete_portfolio", "Delete portfolio position", new BinaryData(JsonSerializer.Serialize(new
        {
            type = "object",
            properties = new { id = new { type = "integer", description = "Portfolio ID" } },
            required = new[] { "id" }
        })));

        yield return new FunctionToolDefinition("get_all_traders", "Get all traders with recommendations and feeds");
        yield return new FunctionToolDefinition("get_trader", "Get trader by ID", new BinaryData(JsonSerializer.Serialize(new
        {
            type = "object",
            properties = new { id = new { type = "integer", description = "Trader ID" } },
            required = new[] { "id" }
        })));
        yield return new FunctionToolDefinition("create_trader", "Create new trader", new BinaryData(JsonSerializer.Serialize(new
        {
            type = "object",
            properties = new
            {
                name = new { type = "string", description = "Trader name" },
                email = new { type = "string", description = "Email" },
                expertise = new { type = "string", description = "Trading expertise" },
                yearsActive = new { type = "string", description = "Years active" }
            },
            required = new[] { "name", "email", "expertise", "yearsActive" }
        })));

        yield return new FunctionToolDefinition("get_all_research_articles", "Get all research articles");
        yield return new FunctionToolDefinition("get_research_article", "Get research article by ID", new BinaryData(JsonSerializer.Serialize(new
        {
            type = "object",
            properties = new { id = new { type = "integer", description = "Article ID" } },
            required = new[] { "id" }
        })));
        yield return new FunctionToolDefinition("create_research_article", "Create new research article", new BinaryData(JsonSerializer.Serialize(new
        {
            type = "object",
            properties = new
            {
                title = new { type = "string", description = "Article title" },
                content = new { type = "string", description = "Article content" },
                currencyPair = new { type = "string", description = "Currency pair analyzed" },
                analysis = new { type = "string", description = "Analysis summary" }
            },
            required = new[] { "title", "content", "currencyPair", "analysis" }
        })));

        yield return new FunctionToolDefinition("get_customer_preferences", "Get customer trading preferences", new BinaryData(JsonSerializer.Serialize(new
        {
            type = "object",
            properties = new { customerId = new { type = "integer", description = "Customer ID" } },
            required = new[] { "customerId" }
        })));
        yield return new FunctionToolDefinition("update_customer_preferences", "Update customer preferences", new BinaryData(JsonSerializer.Serialize(new
        {
            type = "object",
            properties = new
            {
                id = new { type = "integer", description = "Preference ID" },
                customerId = new { type = "integer", description = "Customer ID" },
                riskLevel = new { type = "string", description = "Risk tolerance level" },
                preferredPairs = new { type = "string", description = "Preferred currency pairs" },
                tradingObjective = new { type = "string", description = "Trading objective" }
            },
            required = new[] { "id", "customerId", "riskLevel", "preferredPairs", "tradingObjective" }
        })));

        yield return new FunctionToolDefinition("get_customer_history", "Get customer trading history", new BinaryData(JsonSerializer.Serialize(new
        {
            type = "object",
            properties = new { customerId = new { type = "integer", description = "Customer ID" } },
            required = new[] { "customerId" }
        })));

        yield return new FunctionToolDefinition("get_all_research_drafts", "Get all research drafts");
        yield return new FunctionToolDefinition("create_research_draft", "Create new research draft", new BinaryData(JsonSerializer.Serialize(new
        {
            type = "object",
            properties = new
            {
                title = new { type = "string", description = "Draft title" },
                content = new { type = "string", description = "Draft content" },
                status = new { type = "string", description = "Draft status" }
            },
            required = new[] { "title", "content", "status" }
        })));

        yield return new FunctionToolDefinition("get_all_research_patterns", "Get all identified trading patterns");
        yield return new FunctionToolDefinition("create_research_pattern", "Create new pattern observation", new BinaryData(JsonSerializer.Serialize(new
        {
            type = "object",
            properties = new
            {
                patternName = new { type = "string", description = "Pattern name" },
                description = new { type = "string", description = "Pattern description" },
                currencyPair = new { type = "string", description = "Currency pair" }
            },
            required = new[] { "patternName", "description", "currencyPair" }
        })));

        yield return new FunctionToolDefinition("get_trader_news", "Get news feeds for a trader", new BinaryData(JsonSerializer.Serialize(new
        {
            type = "object",
            properties = new { traderId = new { type = "integer", description = "Trader ID" } },
            required = new[] { "traderId" }
        })));

        yield return new FunctionToolDefinition("get_trader_recommendations", "Get trader recommendations", new BinaryData(JsonSerializer.Serialize(new
        {
            type = "object",
            properties = new { traderId = new { type = "integer", description = "Trader ID" } },
            required = new[] { "traderId" }
        })));

        yield return new FunctionToolDefinition("web_search", "Search the web for current information using Bing", new BinaryData(JsonSerializer.Serialize(new
        {
            type = "object",
            properties = new
            {
                query = new { type = "string", description = "Search query" },
                count = new { type = "integer", description = "Number of results (default 10)" }
            },
            required = new[] { "query" }
        })));

        yield return new FunctionToolDefinition("news_search", "Search news articles using Bing News", new BinaryData(JsonSerializer.Serialize(new
        {
            type = "object",
            properties = new
            {
                query = new { type = "string", description = "News search query" },
                count = new { type = "integer", description = "Number of results (default 10)" }
            },
            required = new[] { "query" }
        })));
    }

    public static async Task<string> InvokeToolAsync(string toolName, JsonElement arguments)
    {
        return toolName switch
        {
            "get_all_customers" => await McpTools.Customers.GetAll(),
            "get_customer" => await McpTools.Customers.Get(arguments.GetProperty("id").GetInt32()),
            "create_customer" => await McpTools.Customers.Create(
                arguments.GetProperty("name").GetString()!,
                arguments.GetProperty("email").GetString()!,
                arguments.GetProperty("phone").GetString()!,
                arguments.GetProperty("company").GetString()!),
            "update_customer" => await McpTools.Customers.Update(
                arguments.GetProperty("id").GetInt32(),
                arguments.GetProperty("name").GetString()!,
                arguments.GetProperty("email").GetString()!,
                arguments.GetProperty("phone").GetString()!,
                arguments.GetProperty("company").GetString()!),
            "delete_customer" => await McpTools.Customers.Delete(arguments.GetProperty("id").GetInt32()),

            "get_customer_portfolios" => await McpTools.Portfolios.GetByCustomer(arguments.GetProperty("customerId").GetInt32()),
            "get_portfolio" => await McpTools.Portfolios.Get(arguments.GetProperty("id").GetInt32()),
            "create_portfolio" => await McpTools.Portfolios.Create(
                arguments.GetProperty("customerId").GetInt32(),
                arguments.GetProperty("currencyPair").GetString()!,
                arguments.GetProperty("direction").GetString()!,
                arguments.GetProperty("amount").GetDecimal(),
                arguments.GetProperty("entryRate").GetDecimal(),
                arguments.TryGetProperty("status", out var status) ? status.GetString()! : "Open"),
            "update_portfolio" => await McpTools.Portfolios.Update(
                arguments.GetProperty("id").GetInt32(),
                arguments.GetProperty("customerId").GetInt32(),
                arguments.GetProperty("currencyPair").GetString()!,
                arguments.GetProperty("direction").GetString()!,
                arguments.GetProperty("amount").GetDecimal(),
                arguments.GetProperty("entryRate").GetDecimal(),
                arguments.GetProperty("status").GetString()!),
            "delete_portfolio" => await McpTools.Portfolios.Delete(arguments.GetProperty("id").GetInt32()),

            "get_all_traders" => await McpTools.Traders.GetAll(),
            "get_trader" => await McpTools.Traders.Get(arguments.GetProperty("id").GetInt32()),
            "create_trader" => await McpTools.Traders.Create(
                arguments.GetProperty("name").GetString()!,
                arguments.GetProperty("email").GetString()!,
                arguments.GetProperty("expertise").GetString()!,
                arguments.GetProperty("yearsActive").GetString()!),

            "get_all_research_articles" => await McpTools.ResearchArticles.GetAll(),
            "get_research_article" => await McpTools.ResearchArticles.Get(arguments.GetProperty("id").GetInt32()),
            "create_research_article" => await McpTools.ResearchArticles.Create(
                arguments.GetProperty("title").GetString()!,
                arguments.GetProperty("content").GetString()!,
                arguments.GetProperty("currencyPair").GetString()!,
                arguments.GetProperty("analysis").GetString()!),

            "get_customer_preferences" => await McpTools.CustomerPreferences.GetByCustomer(arguments.GetProperty("customerId").GetInt32()),
            "update_customer_preferences" => await McpTools.CustomerPreferences.Update(
                arguments.GetProperty("id").GetInt32(),
                arguments.GetProperty("customerId").GetInt32(),
                arguments.GetProperty("riskLevel").GetString()!,
                arguments.GetProperty("preferredPairs").GetString()!,
                arguments.GetProperty("tradingObjective").GetString()!),

            "get_customer_history" => await McpTools.CustomerHistories.GetByCustomer(arguments.GetProperty("customerId").GetInt32()),

            "get_all_research_drafts" => await McpTools.ResearchDrafts.GetAll(),
            "create_research_draft" => await McpTools.ResearchDrafts.Create(
                arguments.GetProperty("title").GetString()!,
                arguments.GetProperty("content").GetString()!,
                arguments.GetProperty("status").GetString()!),

            "get_all_research_patterns" => await McpTools.ResearchPatterns.GetAll(),
            "create_research_pattern" => await McpTools.ResearchPatterns.Create(
                arguments.GetProperty("patternName").GetString()!,
                arguments.GetProperty("description").GetString()!,
                arguments.GetProperty("currencyPair").GetString()!),

            "get_trader_news" => await McpTools.TraderNewsFeeds.GetByTrader(arguments.GetProperty("traderId").GetInt32()),
            "get_trader_recommendations" => await McpTools.TraderRecommendations.GetByTrader(arguments.GetProperty("traderId").GetInt32()),

            "web_search" => await McpTools.BingSearch.SearchWeb(
                arguments.GetProperty("query").GetString()!,
                arguments.TryGetProperty("count", out var webCount) ? webCount.GetInt32() : 10),
            "news_search" => await McpTools.BingSearch.SearchNews(
                arguments.GetProperty("query").GetString()!,
                arguments.TryGetProperty("count", out var newsCount) ? newsCount.GetInt32() : 10),

            _ => JsonSerializer.Serialize(new { error = "Unknown tool" })
        };
    }
}
