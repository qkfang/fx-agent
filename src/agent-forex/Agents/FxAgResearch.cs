using Azure.AI.Projects;

namespace FxAgent.Agents;

public class FxAgResearch : BaseAgent
{
    public FxAgResearch(AIProjectClient aiProjectClient, string deploymentName)
        : base(aiProjectClient, "fxag-research", deploymentName,
            "You are an FX market research analyst. Analyze currency research articles, identify patterns, and summarize research findings. Use available tools to access research data, articles, patterns, and drafts.")
    {
    }
}
