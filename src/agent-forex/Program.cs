using System.ClientModel.Primitives;
using Azure.AI.AgentServer.AgentFramework.Extensions;
using Azure.AI.OpenAI;
using Azure.AI.Projects;
using Azure.AI.Projects.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

var endpoint = Environment.GetEnvironmentVariable("AZURE_AI_PROJECT_ENDPOINT")
    ?? throw new InvalidOperationException("AZURE_AI_PROJECT_ENDPOINT is not set.");
var deploymentName = Environment.GetEnvironmentVariable("MODEL_DEPLOYMENT_NAME") ?? "gpt-4.1";
var crmBrokerUrl = Environment.GetEnvironmentVariable("CRM_BROKER_URL") ?? "http://localhost:5148";

Console.WriteLine($"Project Endpoint: {endpoint}");
Console.WriteLine($"Model Deployment: {deploymentName}");
Console.WriteLine($"CRM Broker URL: {crmBrokerUrl}");

var httpClient = new HttpClient { BaseAddress = new Uri(crmBrokerUrl) };
var tools = new FxTools(httpClient);

var credential = new DefaultAzureCredential();
AIProjectClient projectClient = new AIProjectClient(new Uri(endpoint), credential);

await FoundryProvisioner.ProvisionAsync(projectClient, deploymentName);

ClientConnection connection = projectClient.GetConnection(typeof(AzureOpenAIClient).FullName!);

if (!connection.TryGetLocatorAsUri(out Uri? openAiEndpoint) || openAiEndpoint is null)
{
    throw new InvalidOperationException("Failed to get OpenAI endpoint from project connection.");
}
openAiEndpoint = new Uri($"https://{openAiEndpoint.Host}");
Console.WriteLine($"OpenAI Endpoint: {openAiEndpoint}");

var chatClient = new AzureOpenAIClient(openAiEndpoint, credential)
    .GetChatClient(deploymentName)
    .AsIChatClient()
    .AsBuilder()
    .UseOpenTelemetry(sourceName: "Agents", configure: cfg => cfg.EnableSensitiveData = false)
    .Build();

var agent = new ChatClientAgent(chatClient,
    name: "ForexTradingAgent",
    instructions: """
        You are an expert forex trading assistant for AUD/USD currency pair.

        Your capabilities:
        - View real-time FX quotes (bid/ask/spread)
        - Check market status (trend, volatility, day statistics)
        - View price history candles
        - List trading accounts and their balances
        - Execute buy and sell trades
        - Close open positions
        - View transaction history

        Guidelines:
        - Always show the current quote before executing trades
        - Warn about risks before executing large trades
        - Present account information clearly with key metrics
        - When asked about market conditions, use both quote and status tools
        - Format currency values to 4 decimal places for rates and 2 for account balances
        """,
    tools:
    [
        AIFunctionFactory.Create(tools.GetFxQuote),
        AIFunctionFactory.Create(tools.GetMarketStatus),
        AIFunctionFactory.Create(tools.GetPriceHistory),
        AIFunctionFactory.Create(tools.GetAccounts),
        AIFunctionFactory.Create(tools.GetAccountBalance),
        AIFunctionFactory.Create(tools.ExecuteBuy),
        AIFunctionFactory.Create(tools.ExecuteSell),
        AIFunctionFactory.Create(tools.ClosePosition),
        AIFunctionFactory.Create(tools.GetTransactions)
    ])
    .AsBuilder()
    .UseOpenTelemetry(sourceName: "Agents", configure: cfg => cfg.EnableSensitiveData = false)
    .Build();

Console.WriteLine("Forex Trading Agent Server running on http://localhost:8088");
await agent.RunAIAgentAsync(telemetrySourceName: "Agents");
