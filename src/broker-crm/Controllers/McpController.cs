using Microsoft.AspNetCore.Mvc;
using FxWebApi.Models;
using FxWebApi.Services;

namespace FxWebApi.Controllers
{
    /// <summary>
    /// MCP (Model Context Protocol) endpoint.
    /// Exposes FX trading tools for AI agents / broker-crm integration.
    /// </summary>
    [ApiController]
    [Route("mcp")]
    public class McpController : ControllerBase
    {
        private readonly FxRateService _fxRateService;
        private readonly ILogger<McpController> _logger;

        // Tool manifest returned by GET /mcp/tools
        private static readonly object[] ToolManifest = new object[]
        {
            new {
                name = "fx_quote",
                description = "Get the current AUD/USD bid/ask quote with spread",
                parameters = new { type = "object", properties = new { }, required = Array.Empty<string>() }
            },
            new {
                name = "fx_buy",
                description = "Execute a market buy order for AUD/USD at the current ask price",
                parameters = new {
                    type = "object",
                    properties = new {
                        amount = new { type = "number", description = "Amount in AUD to buy" }
                    },
                    required = new[] { "amount" }
                }
            },
            new {
                name = "fx_sell",
                description = "Execute a market sell order for AUD/USD at the current bid price",
                parameters = new {
                    type = "object",
                    properties = new {
                        amount = new { type = "number", description = "Amount in AUD to sell" }
                    },
                    required = new[] { "amount" }
                }
            },
            new {
                name = "fx_history",
                description = "Get OHLC candlestick price history for AUD/USD",
                parameters = new {
                    type = "object",
                    properties = new {
                        bars = new { type = "integer", description = "Number of candles to return (default 50, max 200)" }
                    },
                    required = Array.Empty<string>()
                }
            },
            new {
                name = "fx_set_trend",
                description = "Simulate a market trend event — triggers a price move in the given direction",
                parameters = new {
                    type = "object",
                    properties = new {
                        direction = new { type = "string", @enum = new[] { "up", "down", "neutral" } },
                        strength  = new { type = "integer", description = "Trend strength 0-100" }
                    },
                    required = new[] { "direction" }
                }
            },
            new {
                name = "fx_market_status",
                description = "Get current market status: trend, volatility, day high/low, active session",
                parameters = new { type = "object", properties = new { }, required = Array.Empty<string>() }
            }
        };

        public McpController(FxRateService fxRateService, ILogger<McpController> logger)
        {
            _fxRateService = fxRateService;
            _logger = logger;
        }

        /// <summary>List available MCP tools.</summary>
        [HttpGet("tools")]
        public ActionResult GetTools()
        {
            return Ok(new { tools = ToolManifest });
        }

        /// <summary>
        /// Generic MCP tool-call endpoint.
        /// Body: { "tool": "fx_buy", "parameters": { "amount": 10000 } }
        /// </summary>
        [HttpPost("call")]
        public ActionResult<McpResponse> Call([FromBody] McpToolCall call)
        {
            _logger.LogInformation("MCP call: {Tool}", call.Tool);
            return call.Tool switch
            {
                "fx_quote"        => Ok(McpOk(_fxRateService.GetCurrentQuote())),
                "fx_buy"          => ExecuteTrade("Buy",  call),
                "fx_sell"         => ExecuteTrade("Sell", call),
                "fx_history"      => Ok(McpOk(_fxRateService.GetHistory(GetInt(call, "bars", 50)))),
                "fx_market_status"=> Ok(McpOk(_fxRateService.GetMarketStatus())),
                "fx_set_trend"    => SetTrendTool(call),
                _ => BadRequest(new McpResponse { Success = false, Message = $"Unknown tool: {call.Tool}" })
            };
        }

        /// <summary>Legacy simple MCP endpoint (buy/sell/quote).</summary>
        [HttpPost("fx")]
        public ActionResult<McpResponse> ExecuteFxAction([FromBody] McpRequest request)
        {
            _logger.LogInformation("MCP FX Action: {Action} Amount: {Amount}", request.Action, request.Amount);

            if (string.IsNullOrEmpty(request.Action))
                return BadRequest(new McpResponse { Success = false, Message = "Action is required." });

            return request.Action.ToLower() switch
            {
                "buy" when request.Amount > 0 =>
                    Ok(Map(_fxRateService.ExecuteTransaction("Buy", request.Amount, "MCP"))),
                "sell" when request.Amount > 0 =>
                    Ok(Map(_fxRateService.ExecuteTransaction("Sell", request.Amount, "MCP"))),
                "quote" =>
                    Ok(McpOk(_fxRateService.GetCurrentQuote())),
                _ => BadRequest(new McpResponse { Success = false, Message = $"Unknown action or missing amount: {request.Action}" })
            };
        }

        /// <summary>MCP service health + current rate.</summary>
        [HttpGet("status")]
        public ActionResult<McpResponse> GetStatus()
        {
            return Ok(new McpResponse
            {
                Success = true,
                Message = "FX MCP Service is running",
                Data = new
                {
                    quote  = _fxRateService.GetCurrentQuote(),
                    market = _fxRateService.GetMarketStatus(),
                    tools  = ToolManifest.Length
                }
            });
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private ActionResult<McpResponse> ExecuteTrade(string type, McpToolCall call)
        {
            if (!call.Parameters.TryGetValue("amount", out var rawAmount) ||
                !decimal.TryParse(rawAmount?.ToString(), out var amount) || amount <= 0)
                return BadRequest(new McpResponse { Success = false, Message = "Parameter 'amount' must be a positive number." });

            var result = _fxRateService.ExecuteTransaction(type, amount, "MCP");
            return Ok(Map(result));
        }

        private ActionResult<McpResponse> SetTrendTool(McpToolCall call)
        {
            var direction = call.Parameters.TryGetValue("direction", out var d) ? d?.ToString() ?? "neutral" : "neutral";
            var strength  = GetInt(call, "strength", 70);
            _fxRateService.SetTrend(direction, strength);
            return Ok(new McpResponse { Success = true, Message = $"Trend set: {direction} @ {strength}%" });
        }

        private static McpResponse Map(FxTransactionResult r) =>
            new() { Success = r.Success, Message = r.Message, Data = r.Record };

        private static McpResponse McpOk(object data) =>
            new() { Success = true, Message = "OK", Data = data };

        private static int GetInt(McpToolCall call, string key, int defaultValue)
        {
            if (call.Parameters.TryGetValue(key, out var v) && int.TryParse(v?.ToString(), out var parsed))
                return parsed;
            return defaultValue;
        }
    }
}
