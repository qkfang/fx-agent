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

var logger = app.Services.GetRequiredService<ILogger<Program>>();

var endpoint = app.Configuration["AZURE_AI_PROJECT_ENDPOINT"]
    ?? throw new InvalidOperationException("AZURE_AI_PROJECT_ENDPOINT is not set.");
var deploymentName = app.Configuration["AZURE_AI_MODEL_DEPLOYMENT_NAME"] ?? "gpt-4.1";

AIProjectClient aiProjectClient = new(new Uri(endpoint), new AzureCliCredential());

var researchAgent = new FxAgResearch(aiProjectClient, deploymentName);

var apiMcpUrl = app.Configuration["API_MCP_URL"] ?? "http://localhost:5005";
var mcpClient = await McpClient.CreateAsync(new HttpClientTransport(new HttpClientTransportOptions
{
    Endpoint = new Uri($"{apiMcpUrl}/mcp"),
    Name = "FxIntegrationApi"
}));
app.Lifetime.ApplicationStopped.Register(() => mcpClient.DisposeAsync().AsTask().Wait());
var mcpTools = await mcpClient.ListToolsAsync();
logger.LogInformation("MCP tools loaded: {Tools}", string.Join(", ", mcpTools.Select(t => t.Name)));

var tradingMcpUrl = app.Configuration["TRADING_MCP_URL"] ?? "http://localhost:5249";
var tradingMcpClient = await McpClient.CreateAsync(new HttpClientTransport(new HttpClientTransportOptions
{
    Endpoint = new Uri($"{tradingMcpUrl}/mcp"),
    Name = "TradingPlatform"
}));
app.Lifetime.ApplicationStopped.Register(() => tradingMcpClient.DisposeAsync().AsTask().Wait());
var tradingMcpTools = await tradingMcpClient.ListToolsAsync();
logger.LogInformation("Trading MCP tools loaded: {Tools}", string.Join(", ", tradingMcpTools.Select(t => t.Name)));

var suggestionAgent = new FxAgSuggestion(aiProjectClient, deploymentName, [.. mcpTools.Cast<AITool>()]);
var traderAgent = new FxAgTrader(aiProjectClient, deploymentName, [.. tradingMcpTools.Cast<AITool>()]);
var insightAgent = new FxAgInsight(aiProjectClient, deploymentName, [.. mcpTools.Cast<AITool>()]);

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
