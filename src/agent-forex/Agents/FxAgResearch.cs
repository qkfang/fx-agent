using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Foundry;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace FxAgent.Agents;

public class FxAgResearch : BaseAgent
{
    public FxAgResearch(AIProjectClient aiProjectClient, string deploymentName, IList<AITool>? tools = null, ILogger? logger = null)
        : base(aiProjectClient, "fxag-research", deploymentName, GetInstructions(), tools, logger)
    {
    }

    private static string GetInstructions() => """
        You are an FX Market Research Analyst Agent specializing in processing breaking forex market news and creating actionable research insights.
        
        - Use `create_research_draft` tool to persist the draft research
        - Confirm successful save and report the article ID
        
        Use your tools systematically and provide thorough, professional forex market analysis.
        """;
}
