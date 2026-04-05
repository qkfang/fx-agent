using System.Text;
using System.Text.Json;

namespace FxWebPortal.Services;

public class ChatService
{
    private readonly HttpClient _http;
    private readonly string _agentEndpoint;
    private readonly ILogger<ChatService> _logger;
    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public ChatService(HttpClient http, IConfiguration config, ILogger<ChatService> logger)
    {
        _http = http;
        _agentEndpoint = config["FoundryAgent:EndpointUrl"]?.TrimEnd('/');
        _logger = logger;
    }

    public async Task<string> SendMessageAsync(string userMessage, List<ChatTurn>? history = null)
    {
        return await SendMessageWithOptionsAsync(userMessage, 0.7, history);
    }

    public async Task<string> SendMessageWithOptionsAsync(string userMessage, double temperature, List<ChatTurn>? history = null)
    {
        var payload = new { message = userMessage, temperature };
        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            var response = await _http.PostAsync($"{_agentEndpoint}/insight", content);
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<AgentInsightResponse>(body, _jsonOptions);
            return result?.Response ?? "No response from agent.";
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get response from Foundry agent");
            return "Sorry, the research assistant is currently unavailable. Please try again later.";
        }
    }
}

public class ChatTurn
{
    public string Role { get; set; } = "";
    public string Content { get; set; } = "";
}

public class AgentInsightResponse
{
    public string? Response { get; set; }
}
