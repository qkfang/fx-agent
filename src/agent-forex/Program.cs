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

AIProjectClient aiProjectClient = new(new Uri(endpoint), new DefaultAzureCredential());

ProjectsAgentVersion agentVersion = await aiProjectClient.AgentAdministrationClient.CreateAgentVersionAsync(
    "ForexTradingAgent",
    new ProjectsAgentVersionCreationOptions(
        new DeclarativeAgentDefinition(model: deploymentName)
        {
            Instructions = """
                You are an expert forex trading assistant specializing in currency markets.

                Your role:
                - Provide insights on forex market trends and analysis
                - Explain currency pair movements and technical indicators
                - Discuss risk management strategies for forex trading
                - Analyze macroeconomic factors affecting exchange rates
                - Share best practices for forex trading

                Guidelines:
                - Provide clear, professional forex market analysis
                - Explain complex concepts in accessible terms
                - Always emphasize risk management and responsible trading
                - Use proper forex terminology and conventions
                - Base responses on sound financial principles
                """
        }));

FoundryAgent agent = aiProjectClient.AsAIAgent(agentVersion);
logger.LogInformation("Agent created. Name: {Name}", agent.Name);

app.MapPost("/chat", async (ChatRequest request) =>
{
    var response = await agent.RunAsync(request.Message);
    return Results.Ok(new { response });
});

app.Lifetime.ApplicationStopping.Register(() =>
{
    aiProjectClient.AgentAdministrationClient.DeleteAgentAsync(agent.Name).GetAwaiter().GetResult();
    logger.LogInformation("Agent deleted: {Name}", agent.Name);
});

await app.RunAsync();

record ChatRequest(string Message);
