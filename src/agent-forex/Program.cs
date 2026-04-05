using Azure.AI.AgentServer.AgentFramework.Extensions;
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

var endpoint = builder.Configuration["AZURE_OPENAI_ENDPOINT"];
if (string.IsNullOrEmpty(endpoint))
    throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT is not set.");

var deploymentName = builder.Configuration["MODEL_DEPLOYMENT_NAME"] ?? "gpt-4.1";

builder.Services.AddSingleton<IChatClient>(sp =>
{
    var credential = new DefaultAzureCredential();
    return new AzureOpenAIClient(new Uri(endpoint), credential)
        .GetChatClient(deploymentName)
        .AsIChatClient()
        .AsBuilder()
        .UseOpenTelemetry(sourceName: "Agents", configure: cfg => cfg.EnableSensitiveData = false)
        .Build();
});

builder.Services.AddSingleton<AIAgent>(sp =>
{
    var chatClient = sp.GetRequiredService<IChatClient>();

    var agent = new ChatClientAgent(chatClient,
        name: "ForexTradingAgent",
        instructions: """
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
            """);

    return agent
        .AsBuilder()
        .UseOpenTelemetry(sourceName: "Agents", configure: cfg => cfg.EnableSensitiveData = false)
        .Build();
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapHealthChecks("/health");

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Forex Trading Agent Server started");
logger.LogInformation("Azure OpenAI Endpoint: {Endpoint}", endpoint);
logger.LogInformation("Model Deployment: {DeploymentName}", deploymentName);

await app.RunAsync();
