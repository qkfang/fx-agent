using FxWebUI.Services;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;

namespace FxWebUI.Mcp;

[McpServerToolType]
public class TradingMcpTools(FxDataService fxData)
{
    [McpServerTool(Name = "get_transactions"), Description("Get trading transaction history with details of all buy and sell activities.")]
    public Task<string> GetTransactions([Description("Maximum number of transactions to return (default 50)")] int limit = 50)
    {
        var transactions = fxData.GetTransactions().Take(limit);
        return Task.FromResult(JsonSerializer.Serialize(transactions));
    }

    [McpServerTool(Name = "get_fund_summary"), Description("Get fund portfolio summary including total balance, AUD balance, USD balance, and profit/loss.")]
    public Task<string> GetFundSummary()
    {
        return Task.FromResult(JsonSerializer.Serialize(fxData.GetFundSummary()));
    }
}
