using Azure.AI.Projects;
using Microsoft.Extensions.AI;

namespace FxAgent.Agents;

public class FxAgTrader : BaseAgent
{
    public FxAgTrader(AIProjectClient aiProjectClient, string deploymentName, IList<AITool>? tools = null)
        : base(aiProjectClient, "fxag-trader", deploymentName,
            "You are an FX trader assistant. Help traders interpret news feeds, evaluate open positions, and support trading decisions. Use trading tools to execute buy/sell orders, get market quotes, check market status, and view price history. Use available tools to access traders, news feeds, recommendations, and customer portfolios.",
            tools)
    {
    }
}
