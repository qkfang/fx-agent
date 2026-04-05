using Azure.AI.Projects;
using Azure.AI.Projects.Agents;
using Azure.Identity;
using Microsoft.Agents.AI.Foundry;

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

var agentDefs = new[]
{
    ("fxag-research",    "You are an FX market research analyst. Analyze currency research articles, identify patterns, and summarize research findings to help traders understand market dynamics."),
    ("fxag-suggestion",  "You are an FX trading suggestion engine. Based on market conditions, news, and portfolio data, provide actionable trading suggestions and recommendations for currency pairs."),
    ("fxag-trader",      "You are an FX trader assistant. Help traders interpret news feeds, evaluate open positions, and support day-to-day trading decisions across currency pairs."),
    ("fxag-insight",     "You are an FX market insight specialist. Deliver concise market insights, portfolio performance summaries, and trend analysis to support strategic investment decisions."),
};

var agentVersionTasks = agentDefs.Select(def =>
    aiProjectClient.AgentAdministrationClient.CreateAgentVersionAsync(
        def.Item1,
        new ProjectsAgentVersionCreationOptions(
            new DeclarativeAgentDefinition(model: deploymentName)
            {
                Instructions = def.Item2,
            })));

var agentVersions = await Task.WhenAll(agentVersionTasks);

FoundryAgent researchAgent    = aiProjectClient.AsAIAgent(agentVersions[0]);
FoundryAgent suggestionAgent  = aiProjectClient.AsAIAgent(agentVersions[1]);
FoundryAgent traderAgent      = aiProjectClient.AsAIAgent(agentVersions[2]);
FoundryAgent insightAgent     = aiProjectClient.AsAIAgent(agentVersions[3]);

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
