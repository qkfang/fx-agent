using Azure.AI.Projects;
using Azure.Identity;
using FxAgent.Agents;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapHealthChecks("/health");

app.MapGet("/", () => Results.Redirect("/swagger"));

var logger = app.Services.GetRequiredService<ILogger<Program>>();

var endpoint = app.Configuration["AZURE_AI_PROJECT_ENDPOINT"]
    ?? throw new InvalidOperationException("AZURE_AI_PROJECT_ENDPOINT is not set.");
var deploymentName = app.Configuration["AZURE_AI_MODEL_DEPLOYMENT_NAME"];

AIProjectClient aiProjectClient = new(new Uri(endpoint), new AzureCliCredential());


var apiMcpUrl = app.Configuration["API_INTG_MCP_URL"];
var mcpClient = await McpClient.CreateAsync(new HttpClientTransport(new HttpClientTransportOptions
{
    Endpoint = new Uri($"{apiMcpUrl}/mcp"),
    Name = "FxIntegrationApi"
}));
app.Lifetime.ApplicationStopped.Register(() => mcpClient.DisposeAsync().AsTask().Wait());
var mcpTools = await mcpClient.ListToolsAsync();
logger.LogInformation("MCP tools loaded: {Tools}", string.Join(", ", mcpTools.Select(t => t.Name)));

var researchToolNames = new[] { "create_research_draft", "get_research_article", "create_research_article", 
    "get_all_research_patterns", "get_all_research_drafts", "get_all_research_articles", "create_research_pattern" };
var researchTools = mcpTools.Where(t => researchToolNames.Contains(t.Name)).ToList();

var suggestionToolNames = new[] { 
    "create_research_draft", "get_research_article", "create_research_article", 
    "get_all_research_patterns", "get_all_research_drafts", "get_all_research_articles", "create_research_pattern",
    "delete_customer", "get_all_customers", "get_customer_preferences", "update_customer_preferences", 
    "get_customer_history", "update_customer", "get_customer_portfolios", "create_customer", "get_customer",
    "create_trader", "get_trader_news", "get_trader", "get_all_traders", "get_trader_recommendations",
    "update_portfolio", "create_portfolio", "get_portfolio", "delete_portfolio" };
var suggestionTools = mcpTools.Where(t => suggestionToolNames.Contains(t.Name)).ToList();

var tradingMcpUrl = app.Configuration["TRADING_PLATFORM_MCP_URL"];
var tradingMcpClient = await McpClient.CreateAsync(new HttpClientTransport(new HttpClientTransportOptions
{
    Endpoint = new Uri($"{tradingMcpUrl}/mcp"),
    Name = "TradingPlatform"
}));
app.Lifetime.ApplicationStopped.Register(() => tradingMcpClient.DisposeAsync().AsTask().Wait());
var tradingMcpTools = await tradingMcpClient.ListToolsAsync();
logger.LogInformation("Trading MCP tools loaded: {Tools}", string.Join(", ", tradingMcpTools.Select(t => t.Name)));

// Create research agent with MCP tools and web search
var researchAgent = new FxAgResearch(aiProjectClient, deploymentName, [.. researchTools.Cast<AITool>(), new HostedWebSearchTool()]);
var suggestionAgent = new FxAgSuggestion(aiProjectClient, deploymentName, [.. suggestionTools.Cast<AITool>()]);
var insightAgent = new FxAgInsight(aiProjectClient, deploymentName, [.. mcpTools.Cast<AITool>()]);
var traderAgent = new FxAgTrader(aiProjectClient, deploymentName, [.. tradingMcpTools.Cast<AITool>()]);

logger.LogInformation("Agents created: fxag-research, fxag-suggestion, fxag-trader, fxag-insight");

app.MapPost("/research", async (ChatRequest request) =>
{
    var response = await researchAgent.RunAsync(request.Message);
    return Results.Ok(new { response });
});

app.MapPost("/suggestion", async (ChatRequest request) =>
{
    var response = await suggestionAgent.RunAsync(request.Message);
    return Results.Ok(new { response });
});

app.MapPost("/trader", async (ChatRequest request) =>
{
    var response = await traderAgent.RunAsync(request.Message);
    return Results.Ok(new { response });
});

app.MapPost("/insight", async (ChatRequest request) =>
{
    var response = await insightAgent.RunAsync(request.Message);
    return Results.Ok(new { response });
});

await app.RunAsync();

record ChatRequest(string Message);
