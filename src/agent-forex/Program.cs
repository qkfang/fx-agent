using Azure.AI.Projects;
using Azure.Identity;
using FxAgent.Agents;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationInsightsTelemetry();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.AddApplicationInsights();

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
var deploymentName = app.Configuration["AZURE_AI_MODEL_DEPLOYMENT_NAME"]
    ?? throw new InvalidOperationException("AZURE_AI_MODEL_DEPLOYMENT_NAME is not set.");

logger.LogInformation("Using AI Project endpoint: {Endpoint}", endpoint);
logger.LogInformation("Using model deployment: {Deployment}", deploymentName);

AIProjectClient aiProjectClient = new(new Uri(endpoint), new AzureCliCredential());


logger.LogInformation("Connecting to API Integration MCP server at {Url}", app.Configuration["API_INTG_MCP_URL"]);
var apiMcpUrl = app.Configuration["API_INTG_MCP_URL"];
var mcpClient = await McpClient.CreateAsync(new HttpClientTransport(new HttpClientTransportOptions
{
    Endpoint = new Uri($"{apiMcpUrl}/mcp"),
    Name = "FxIntegrationApi"
}));
app.Lifetime.ApplicationStopped.Register(() => mcpClient.DisposeAsync().AsTask().Wait());
var mcpTools = await mcpClient.ListToolsAsync();
logger.LogInformation("API Integration MCP tools loaded ({Count}): {Tools}", mcpTools.Count, string.Join(", ", mcpTools.Select(t => t.Name)));

var researchToolNames = new[] { "create_research_draft", "get_research_article", "create_research_article", 
    "get_all_research_patterns", "get_all_research_drafts", "get_all_research_articles", "create_research_pattern" };
var researchTools = mcpTools.Where(t => researchToolNames.Contains(t.Name)).ToList();
logger.LogDebug("Research agent tools ({Count}): {Tools}", researchTools.Count, string.Join(", ", researchTools.Select(t => t.Name)));

var suggestionToolNames = new[] { 
    "create_research_draft", "get_research_article", "create_research_article", 
    "get_all_research_patterns", "get_all_research_drafts", "get_all_research_articles", "create_research_pattern",
    "delete_customer", "get_all_customers", "get_customer_preferences", "update_customer_preferences", 
    "get_customer_history", "update_customer", "get_customer_portfolios", "create_customer", "get_customer",
    "create_trader", "get_trader_news", "get_trader", "get_all_traders", "get_trader_recommendations",
    "update_portfolio", "create_portfolio", "get_portfolio", "delete_portfolio" };
var suggestionTools = mcpTools.Where(t => suggestionToolNames.Contains(t.Name)).ToList();
logger.LogDebug("Suggestion agent tools ({Count}): {Tools}", suggestionTools.Count, string.Join(", ", suggestionTools.Select(t => t.Name)));

logger.LogInformation("Connecting to Trading Platform MCP server at {Url}", app.Configuration["TRADING_PLATFORM_MCP_URL"]);
var tradingMcpUrl = app.Configuration["TRADING_PLATFORM_MCP_URL"];
var tradingMcpClient = await McpClient.CreateAsync(new HttpClientTransport(new HttpClientTransportOptions
{
    Endpoint = new Uri($"{tradingMcpUrl}/mcp"),
    Name = "TradingPlatform"
}));
app.Lifetime.ApplicationStopped.Register(() => tradingMcpClient.DisposeAsync().AsTask().Wait());
var tradingMcpTools = await tradingMcpClient.ListToolsAsync();
logger.LogInformation("Trading Platform MCP tools loaded ({Count}): {Tools}", tradingMcpTools.Count, string.Join(", ", tradingMcpTools.Select(t => t.Name)));

var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();

// Create research agent with MCP tools and web search
logger.LogInformation("Creating research agent with {ToolCount} tools (including web search)", researchTools.Count + 1);
var researchAgent = new FxAgResearch(aiProjectClient, deploymentName, [.. researchTools.Cast<AITool>(), new HostedWebSearchTool()], loggerFactory.CreateLogger<FxAgResearch>());

logger.LogInformation("Creating suggestion agent with {ToolCount} tools", suggestionTools.Count);
var suggestionAgent = new FxAgSuggestion(aiProjectClient, deploymentName, [.. suggestionTools.Cast<AITool>()], loggerFactory.CreateLogger<FxAgSuggestion>());

logger.LogInformation("Creating insight agent with {ToolCount} tools", mcpTools.Count);
var insightAgent = new FxAgInsight(aiProjectClient, deploymentName, [.. mcpTools.Cast<AITool>()], loggerFactory.CreateLogger<FxAgInsight>());

logger.LogInformation("Creating trader agent with {ToolCount} tools", tradingMcpTools.Count);
var traderAgent = new FxAgTrader(aiProjectClient, deploymentName, [.. tradingMcpTools.Cast<AITool>()], loggerFactory.CreateLogger<FxAgTrader>());

logger.LogInformation("All agents initialized successfully: fxag-research, fxag-suggestion, fxag-trader, fxag-insight");

app.MapPost("/research", async (ChatRequest request) =>
{
    logger.LogInformation("Received research request: {Message}", request.Message);
    var response = await researchAgent.RunAsync(request.Message);
    logger.LogInformation("Research request completed, response length: {Length}", response.Length);
    return Results.Ok(new { response });
});

app.MapPost("/suggestion", async (ChatRequest request) =>
{
    logger.LogInformation("Received suggestion request: {Message}", request.Message);
    var response = await suggestionAgent.RunAsync(request.Message);
    logger.LogInformation("Suggestion request completed, response length: {Length}", response.Length);
    return Results.Ok(new { response });
});

app.MapPost("/trader", async (ChatRequest request) =>
{
    logger.LogInformation("Received trader request: {Message}", request.Message);
    var response = await traderAgent.RunAsync(request.Message);
    logger.LogInformation("Trader request completed, response length: {Length}", response.Length);
    return Results.Ok(new { response });
});

app.MapPost("/insight", async (ChatRequest request) =>
{
    logger.LogInformation("Received insight request: {Message}", request.Message);
    var response = await insightAgent.RunAsync(request.Message);
    logger.LogInformation("Insight request completed, response length: {Length}", response.Length);
    return Results.Ok(new { response });
});

await app.RunAsync();

record ChatRequest(string Message);
