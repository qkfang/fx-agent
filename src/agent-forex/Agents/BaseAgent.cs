using Azure.AI.Projects;
using Azure.AI.Projects.Agents;
using Azure.AI.Agents.Persistent;
using Microsoft.Agents.AI.Foundry;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using OpenAI.Responses;
using System.Diagnostics;
using System.Text.Json;

namespace FxAgent.Agents;

public abstract class BaseAgent
{
    protected readonly FoundryAgent _agent;
    protected readonly AIProjectClient _aiProjectClient;
    protected readonly ILogger _logger;
    protected readonly string _agentId;

    protected BaseAgent(AIProjectClient aiProjectClient, string agentId, string deploymentName, string instructions, IList<AITool>? tools = null, ILogger? logger = null)
    {
        _aiProjectClient = aiProjectClient;
        _agentId = agentId;
        _logger = logger ?? LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger(agentId);
        
        _logger.LogInformation("Initializing agent: {AgentId} with deployment: {Deployment}", agentId, deploymentName);
        _logger.LogDebug("Agent instructions length: {Length} chars", instructions.Length);
        
        if (tools != null && tools.Count > 0)
        {
            _logger.LogInformation("Agent {AgentId} configured with {ToolCount} tools", agentId, tools.Count);
        }
        
        var agentDefinition = new DeclarativeAgentDefinition(model: deploymentName)
        {
            Instructions = instructions
        };
        
        var agentVersion = aiProjectClient.AgentAdministrationClient.CreateAgentVersion(
            agentId,
            new ProjectsAgentVersionCreationOptions(agentDefinition));

        _agent = tools != null
            ? aiProjectClient.AsAIAgent(agentVersion, tools)
            : aiProjectClient.AsAIAgent(agentVersion);
            
        _logger.LogInformation("Agent {AgentId} initialized successfully", agentId);
    }

    public async Task<string> RunAsync(string message)
    {
        var requestId = Guid.NewGuid().ToString("N")[..8];
        var sw = Stopwatch.StartNew();
        
        _logger.LogInformation("[{RequestId}] Agent {AgentId} received message: {Message}", requestId, _agentId, message);
        _logger.LogDebug("[{RequestId}] Message length: {Length} chars", requestId, message.Length);
        
        try
        {
            _logger.LogDebug("[{RequestId}] Starting agent execution...", requestId);
            
            var response = await _agent.RunAsync(message);
            
            sw.Stop();
            _logger.LogInformation("[{RequestId}] Agent {AgentId} completed in {Duration}ms", 
                requestId, _agentId, sw.ElapsedMilliseconds);
            _logger.LogDebug("[{RequestId}] Response length: {Length} chars", requestId, response.Text.Length);
            _logger.LogTrace("[{RequestId}] Response content: {Response}", requestId, response.Text);
            
            return response.Text;
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(ex, "[{RequestId}] Agent {AgentId} failed after {Duration}ms: {Error}", 
                requestId, _agentId, sw.ElapsedMilliseconds, ex.Message);
            throw;
        }
    }
}
