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

const string JokerName = "JokerAgent";
// Create a server-side agent version using the native SDK.
ProjectsAgentVersion agentVersion = await aiProjectClient.AgentAdministrationClient.CreateAgentVersionAsync(
    JokerName,
    new ProjectsAgentVersionCreationOptions(
        new DeclarativeAgentDefinition(model: deploymentName)
        {
            Instructions = "You are good at telling jokes.",
        }));
        
// Wrap the agent version as a FoundryAgent using the AsAIAgent extension.
FoundryAgent agent = aiProjectClient.AsAIAgent(agentVersion);

// Once you have the agent, you can invoke it like any other AIAgent.
Console.WriteLine(await agent.RunAsync("Tell me a joke about a pirate."));

app.MapPost("/chat", async (ChatRequest request) =>
{
    var response = await agent.RunAsync(request.Message);
    return Results.Ok(new { response });
});

app.Lifetime.ApplicationStopping.Register(() =>
{
    aiProjectClient.AgentAdministrationClient.DeleteAgentAsync(JokerName).GetAwaiter().GetResult();
    logger.LogInformation("Agent deleted: {Name}", JokerName);
});

await app.RunAsync();

record ChatRequest(string Message);
