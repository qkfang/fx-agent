using System.ComponentModel;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.ClientModel.Primitives;
using Azure.AI.AgentServer.AgentFramework.Extensions;
using Azure.AI.OpenAI;
using Azure.AI.Projects;
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
var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

// ── Tool functions that call the crm-broker API ──────────────────────────

[Description("Get the current AUD/USD exchange rate quote with bid, ask, mid, and spread.")]
string GetFxQuote()
{
    try
    {
        var response = httpClient.GetAsync("/api/fx/quote").Result;
        response.EnsureSuccessStatusCode();
        return response.Content.ReadAsStringAsync().Result;
    }
    catch (Exception ex)
    {
        return $"Error fetching quote: {ex.Message}";
    }
}

[Description("Get the current market status including trend direction, volatility, and day statistics for AUD/USD.")]
string GetMarketStatus()
{
    try
    {
        var response = httpClient.GetAsync("/api/fx/status").Result;
        response.EnsureSuccessStatusCode();
        return response.Content.ReadAsStringAsync().Result;
    }
    catch (Exception ex)
    {
        return $"Error fetching market status: {ex.Message}";
    }
}

[Description("Get recent OHLC price history candles for AUD/USD.")]
string GetPriceHistory(
    [Description("Number of candle bars to retrieve (1-500, default 20)")] int bars = 20)
{
    try
    {
        bars = Math.Clamp(bars, 1, 500);
        var response = httpClient.GetAsync($"/api/fx/history?bars={bars}").Result;
        response.EnsureSuccessStatusCode();
        return response.Content.ReadAsStringAsync().Result;
    }
    catch (Exception ex)
    {
        return $"Error fetching price history: {ex.Message}";
    }
}

[Description("Get all trading accounts with their summary information.")]
string GetAccounts()
{
    try
    {
        var response = httpClient.GetAsync("/api/accounts").Result;
        response.EnsureSuccessStatusCode();
        return response.Content.ReadAsStringAsync().Result;
    }
    catch (Exception ex)
    {
        return $"Error fetching accounts: {ex.Message}";
    }
}

[Description("Get the balance sheet for a specific trading account, including open positions and recent transactions.")]
string GetAccountBalance(
    [Description("The account ID (e.g., 1, 2, or 3)")] int accountId)
{
    try
    {
        var response = httpClient.GetAsync($"/api/accounts/{accountId}/balance").Result;
        response.EnsureSuccessStatusCode();
        return response.Content.ReadAsStringAsync().Result;
    }
    catch (Exception ex)
    {
        return $"Error fetching balance for account {accountId}: {ex.Message}";
    }
}

[Description("Execute a buy trade on AUD/USD for a specific account.")]
string ExecuteBuy(
    [Description("The account ID")] int accountId,
    [Description("Number of lots to buy (e.g., 0.1, 0.5, 1.0)")] decimal lots)
{
    try
    {
        var content = JsonContent.Create(new { currencyPair = "AUD/USD", lots });
        var response = httpClient.PostAsync($"/api/accounts/{accountId}/buy", content).Result;
        return response.Content.ReadAsStringAsync().Result;
    }
    catch (Exception ex)
    {
        return $"Error executing buy: {ex.Message}";
    }
}

[Description("Execute a sell trade on AUD/USD for a specific account.")]
string ExecuteSell(
    [Description("The account ID")] int accountId,
    [Description("Number of lots to sell (e.g., 0.1, 0.5, 1.0)")] decimal lots)
{
    try
    {
        var content = JsonContent.Create(new { currencyPair = "AUD/USD", lots });
        var response = httpClient.PostAsync($"/api/accounts/{accountId}/sell", content).Result;
        return response.Content.ReadAsStringAsync().Result;
    }
    catch (Exception ex)
    {
        return $"Error executing sell: {ex.Message}";
    }
}

[Description("Close an open position for an account.")]
string ClosePosition(
    [Description("The account ID")] int accountId,
    [Description("The position ID to close (e.g., POS001)")] string positionId)
{
    try
    {
        var response = httpClient.PostAsync($"/api/accounts/{accountId}/close/{positionId}", null).Result;
        return response.Content.ReadAsStringAsync().Result;
    }
    catch (Exception ex)
    {
        return $"Error closing position: {ex.Message}";
    }
}

[Description("Get recent FX transaction records.")]
string GetTransactions(
    [Description("Maximum number of transactions to return (default 20)")] int limit = 20)
{
    try
    {
        var response = httpClient.GetAsync($"/api/fx/transactions?limit={limit}").Result;
        response.EnsureSuccessStatusCode();
        return response.Content.ReadAsStringAsync().Result;
    }
    catch (Exception ex)
    {
        return $"Error fetching transactions: {ex.Message}";
    }
}

// ── Build the Agent ──────────────────────────────────────────────────────

var credential = new DefaultAzureCredential();
AIProjectClient projectClient = new AIProjectClient(new Uri(endpoint), credential);

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
        AIFunctionFactory.Create(GetFxQuote),
        AIFunctionFactory.Create(GetMarketStatus),
        AIFunctionFactory.Create(GetPriceHistory),
        AIFunctionFactory.Create(GetAccounts),
        AIFunctionFactory.Create(GetAccountBalance),
        AIFunctionFactory.Create(ExecuteBuy),
        AIFunctionFactory.Create(ExecuteSell),
        AIFunctionFactory.Create(ClosePosition),
        AIFunctionFactory.Create(GetTransactions)
    ])
    .AsBuilder()
    .UseOpenTelemetry(sourceName: "Agents", configure: cfg => cfg.EnableSensitiveData = false)
    .Build();

Console.WriteLine("Forex Trading Agent Server running on http://localhost:8088");
await agent.RunAIAgentAsync(telemetrySourceName: "Agents");
