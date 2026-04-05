using Microsoft.AspNetCore.Mvc;
using FxWebUI.Models;
using FxWebUI.Services;

namespace FxWebUI.Controllers
{
    [ApiController]
    [Route("mcp")]
    public class McpController : ControllerBase
    {
        private readonly FxDataService _dataService;
        private readonly ILogger<McpController> _logger;

        private static readonly object[] ToolManifest = new object[]
        {
            new {
                name = "get_transactions",
                description = "Get trading transaction history with details of all buy and sell activities",
                parameters = new {
                    type = "object",
                    properties = new {
                        limit = new { type = "integer", description = "Maximum number of transactions to return (default 50)" }
                    },
                    required = Array.Empty<string>()
                }
            },
            new {
                name = "get_fund_summary",
                description = "Get fund portfolio summary including total balance, AUD balance, USD balance, and profit/loss",
                parameters = new { type = "object", properties = new { }, required = Array.Empty<string>() }
            },
            new {
                name = "get_latest_transaction",
                description = "Get the most recent transaction",
                parameters = new { type = "object", properties = new { }, required = Array.Empty<string>() }
            },
            new {
                name = "get_transaction_by_id",
                description = "Get a specific transaction by ID",
                parameters = new {
                    type = "object",
                    properties = new {
                        id = new { type = "integer", description = "Transaction ID" }
                    },
                    required = new[] { "id" }
                }
            }
        };

        public McpController(FxDataService dataService, ILogger<McpController> logger)
        {
            _dataService = dataService;
            _logger = logger;
        }

        [HttpGet("tools")]
        public ActionResult GetTools()
        {
            return Ok(new { tools = ToolManifest });
        }

        [HttpPost("call")]
        public ActionResult<McpResponse> Call([FromBody] McpToolCall call)
        {
            _logger.LogInformation("MCP call: {Tool}", call.Tool);
            
            return call.Tool switch
            {
                "get_transactions" => GetTransactionsTool(call),
                "get_fund_summary" => GetFundSummaryTool(),
                "get_latest_transaction" => GetLatestTransactionTool(),
                "get_transaction_by_id" => GetTransactionByIdTool(call),
                _ => BadRequest(new McpResponse { Success = false, Message = $"Unknown tool: {call.Tool}" })
            };
        }

        [HttpGet("status")]
        public ActionResult<McpResponse> GetStatus()
        {
            var transactions = _dataService.GetTransactions();
            var summary = _dataService.GetFundSummary();
            
            return Ok(new McpResponse
            {
                Success = true,
                Message = "Trading Platform MCP Service is running",
                Data = new
                {
                    totalTransactions = transactions.Count,
                    totalBalance = summary.TotalBalance,
                    audBalance = summary.AudBalance,
                    usdBalance = summary.UsdBalance,
                    profitLoss = summary.TotalProfitLoss,
                    tools = ToolManifest.Length
                }
            });
        }

        private ActionResult<McpResponse> GetTransactionsTool(McpToolCall call)
        {
            var limit = GetInt(call, "limit", 50);
            var transactions = _dataService.GetTransactions().Take(limit).ToList();
            
            return Ok(new McpResponse
            {
                Success = true,
                Message = $"Retrieved {transactions.Count} transactions",
                Data = transactions
            });
        }

        private ActionResult<McpResponse> GetFundSummaryTool()
        {
            var summary = _dataService.GetFundSummary();
            
            return Ok(new McpResponse
            {
                Success = true,
                Message = "Fund summary retrieved",
                Data = summary
            });
        }

        private ActionResult<McpResponse> GetLatestTransactionTool()
        {
            var latest = _dataService.GetTransactions().FirstOrDefault();
            
            if (latest == null)
            {
                return NotFound(new McpResponse
                {
                    Success = false,
                    Message = "No transactions found"
                });
            }
            
            return Ok(new McpResponse
            {
                Success = true,
                Message = "Latest transaction retrieved",
                Data = latest
            });
        }

        private ActionResult<McpResponse> GetTransactionByIdTool(McpToolCall call)
        {
            if (!call.Parameters.TryGetValue("id", out var rawId) ||
                !int.TryParse(rawId?.ToString(), out var id))
            {
                return BadRequest(new McpResponse
                {
                    Success = false,
                    Message = "Parameter 'id' must be a valid integer"
                });
            }
            
            var transaction = _dataService.GetTransactions().FirstOrDefault(t => t.Id == id);
            
            if (transaction == null)
            {
                return NotFound(new McpResponse
                {
                    Success = false,
                    Message = $"Transaction with ID {id} not found"
                });
            }
            
            return Ok(new McpResponse
            {
                Success = true,
                Message = $"Transaction {id} retrieved",
                Data = transaction
            });
        }

        private static int GetInt(McpToolCall call, string key, int defaultValue)
        {
            if (call.Parameters.TryGetValue(key, out var v) && int.TryParse(v?.ToString(), out var parsed))
                return parsed;
            return defaultValue;
        }
    }

    public class McpToolCall
    {
        public string Tool { get; set; } = string.Empty;
        public Dictionary<string, object> Parameters { get; set; } = new();
    }

    public class McpResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public object? Data { get; set; }
    }
}
