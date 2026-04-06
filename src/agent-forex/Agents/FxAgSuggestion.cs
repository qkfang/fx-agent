using Azure.AI.Projects;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace FxAgent.Agents;

public class FxAgSuggestion : BaseAgent
{
    public FxAgSuggestion(AIProjectClient aiProjectClient, string deploymentName, IList<AITool>? tools = null, ILogger? logger = null)
        : base(aiProjectClient, "fxag-suggestion", deploymentName,
            "You are an FX trading suggestion engine. Based on market conditions, news, and portfolio data, provide actionable trading suggestions. Use trading tools to check current quotes and market status before making recommendations. Use available tools to access customer preferences, portfolios, trader recommendations, and research insights.",
            tools, logger)
    {
    }
}
