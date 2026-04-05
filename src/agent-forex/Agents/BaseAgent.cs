using Azure.AI.Projects;
using Azure.AI.Projects.Agents;
using Azure.AI.Agents.Persistent;
using Microsoft.Agents.AI.Foundry;
using OpenAI.Responses;
using System.Text.Json;

namespace FxAgent.Agents;

public abstract class BaseAgent
{
    protected readonly FoundryAgent _agent;
    protected readonly AIProjectClient _aiProjectClient;

    protected BaseAgent(AIProjectClient aiProjectClient, string agentId, string deploymentName, string instructions)
    {
        _aiProjectClient = aiProjectClient;
        
        var agentDefinition = new DeclarativeAgentDefinition(model: deploymentName)
        {
            Instructions = instructions
        };
        
        var agentVersion = aiProjectClient.AgentAdministrationClient.CreateAgentVersion(
            agentId,
            new ProjectsAgentVersionCreationOptions(agentDefinition));

        _agent = aiProjectClient.AsAIAgent(agentVersion);
    }

    public async Task<string> RunAsync(string message)
    {
        var response = await _agent.RunAsync(message);
        return ""; 
    }
}
