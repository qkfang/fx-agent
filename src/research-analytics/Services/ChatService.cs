using System.Text;
using System.Text.Json;

namespace FxWebPortal.Services;

public class ChatService
{
    private readonly HttpClient _http;
    private readonly string _agentEndpoint;
    private readonly string _agentName;
    private readonly ILogger<ChatService> _logger;
    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public ChatService(HttpClient http, IConfiguration config, ILogger<ChatService> logger)
    {
        _http = http;
        _agentEndpoint = config["FoundryAgent:EndpointUrl"]?.TrimEnd('/') ?? "http://localhost:8088";
        _agentName = config["FoundryAgent:AgentName"] ?? "forex-trading-agent";
        _logger = logger;
    }

    public async Task<string> SendMessageAsync(string userMessage, List<ChatTurn>? history = null)
    {
        var input = new List<object>();

        if (history != null)
        {
            foreach (var turn in history)
            {
                input.Add(new { role = turn.Role, content = turn.Content });
            }
        }

        input.Add(new { role = "user", content = userMessage });

        var payload = new { model = _agentName, input };
        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            var response = await _http.PostAsync($"{_agentEndpoint}/v1/responses", content);
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<AgentResponse>(body, _jsonOptions);

            var assistantMessage = result?.Output?
                .Where(o => o.Type == "message" && o.Role == "assistant")
                .SelectMany(o => o.Content ?? Array.Empty<ContentBlock>())
                .Where(c => c.Type == "output_text")
                .Select(c => c.Text)
                .FirstOrDefault();

            return assistantMessage ?? "No response from agent.";
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

public class AgentResponse
{
    public List<OutputItem>? Output { get; set; }
}

public class OutputItem
{
    public string? Type { get; set; }
    public string? Role { get; set; }
    public ContentBlock[]? Content { get; set; }
}

public class ContentBlock
{
    public string? Type { get; set; }
    public string? Text { get; set; }
}
