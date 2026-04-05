using Azure.AI.Projects;
using Azure.AI.Projects.Agents;
using Microsoft.Agents.AI.Foundry;
using System.Text.Json;

namespace FxAgent.Agents;

public abstract class BaseAgent
{
    protected readonly FoundryAgent _agent;
    protected readonly AIProjectClient _aiProjectClient;

    protected BaseAgent(AIProjectClient aiProjectClient, string agentId, string deploymentName, string instructions)
    {
        _aiProjectClient = aiProjectClient;
        
        var tools = McpToolDefinitions.GetAllToolDefinitions().ToList();
        
        var agentVersion = aiProjectClient.AgentAdministrationClient.CreateAgentVersion(
            agentId,
            new ProjectsAgentVersionCreationOptions(
                new DeclarativeAgentDefinition(model: deploymentName)
                {
                    Instructions = instructions,
                    Tools = tools
                }));

        _agent = aiProjectClient.AsAIAgent(agentVersion);
    }

    public async Task<string> RunAsync(string message)
    {
        var threadId = Guid.NewGuid().ToString();
        var thread = await _aiProjectClient.AgentThreadsClient.CreateThreadAsync();
        
        await _aiProjectClient.AgentThreadsClient.CreateMessageAsync(thread.Value.Id, MessageRole.User, message);
        
        var run = await _aiProjectClient.AgentThreadsClient.CreateRunAsync(
            thread.Value.Id,
            _agent.AgentId);

        while (run.Value.Status == RunStatus.Queued || 
               run.Value.Status == RunStatus.InProgress ||
               run.Value.Status == RunStatus.RequiresAction)
        {
            await Task.Delay(500);
            run = await _aiProjectClient.AgentThreadsClient.GetRunAsync(thread.Value.Id, run.Value.Id);

            if (run.Value.Status == RunStatus.RequiresAction &&
                run.Value.RequiredAction is SubmitToolOutputsAction toolAction)
            {
                var toolOutputs = new List<ToolOutput>();
                
                foreach (var toolCall in toolAction.ToolCalls)
                {
                    if (toolCall is RequiredFunctionToolCall funcCall)
                    {
                        var args = JsonDocument.Parse(funcCall.Arguments).RootElement;
                        var output = await McpToolDefinitions.InvokeToolAsync(funcCall.Name, args);
                        toolOutputs.Add(new ToolOutput(funcCall.Id, output));
                    }
                }

                await _aiProjectClient.AgentThreadsClient.SubmitToolOutputsToRunAsync(
                    thread.Value.Id,
                    run.Value.Id,
                    toolOutputs);
            }
        }

        if (run.Value.Status == RunStatus.Completed)
        {
            var messages = await _aiProjectClient.AgentThreadsClient.GetMessagesAsync(thread.Value.Id);
            var lastMessage = messages.Value.Data.FirstOrDefault(m => m.Role == MessageRole.Assistant);
            
            if (lastMessage?.ContentItems.FirstOrDefault() is MessageTextContent textContent)
            {
                return textContent.Text;
            }
        }

        return "Agent run failed or produced no output.";
    }
}
