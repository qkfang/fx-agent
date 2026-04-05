using Microsoft.AspNetCore.Mvc;
using FxWebApi.Models;
using FxWebApi.Services;

namespace FxWebApi.Controllers
{
    /// <summary>
    /// MCP (Model Context Protocol) endpoint.
    /// Exposes FX trading and customer CRM tools for AI agents.
    /// </summary>
    [ApiController]
    [Route("mcp")]
    public class McpController : ControllerBase
    {
        private readonly FxRateService _fxRateService;
        private readonly CustomerService _customerService;
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
            },
            new {
                name = "customer_get",
                description = "Get customer information by ID",
                parameters = new {
                    type = "object",
                    properties = new {
                        customer_id = new { type = "integer", description = "Customer ID" }
                    },
                    required = new[] { "customer_id" }
                }
            },
            new {
                name = "customer_list",
                description = "List all customers",
                parameters = new { type = "object", properties = new { }, required = Array.Empty<string>() }
            },
            new {
                name = "customer_portfolios",
                description = "Get customer's open positions/portfolios",
                parameters = new {
                    type = "object",
                    properties = new {
                        customer_id = new { type = "integer", description = "Customer ID" }
                    },
                    required = new[] { "customer_id" }
                }
            },
            new {
                name = "customer_history",
                description = "Get customer's trading history",
                parameters = new {
                    type = "object",
                    properties = new {
                        customer_id = new { type = "integer", description = "Customer ID" }
                    },
                    required = new[] { "customer_id" }
                }
            },
            new {
                name = "customer_preferences",
                description = "Get customer's trading preferences and risk settings",
                parameters = new {
                    type = "object",
                    properties = new {
                        customer_id = new { type = "integer", description = "Customer ID" }
                    },
                    required = new[] { "customer_id" }
                }
            },
            new {
                name = "customer_profile",
                description = "Get complete customer profile including info, portfolios, history, and preferences",
                parameters = new {
                    type = "object",
                    properties = new {
                        customer_id = new { type = "integer", description = "Customer ID" }
                    },
                    required = new[] { "customer_id" }
                }
            }
        };

        public McpController(FxRateService fxRateService, CustomerService customerService, ILogger<McpController> logger)
        {
            _fxRateService = fxRateService;
            _customerService = customerService;
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
        public async Task<ActionResult<McpResponse>> Call([FromBody] McpToolCall call)
        {
            _logger.LogInformation("MCP call: {Tool}", call.Tool);
            return call.Tool switch
            {
                "fx_quote"             => Ok(McpOk(_fxRateService.GetCurrentQuote())),
                "fx_buy"               => ExecuteTrade("Buy",  call),
                "fx_sell"              => ExecuteTrade("Sell", call),
                "fx_history"           => Ok(McpOk(_fxRateService.GetHistory(GetInt(call, "bars", 50)))),
                "fx_market_status"     => Ok(McpOk(_fxRateService.GetMarketStatus())),
                "fx_set_trend"         => SetTrendTool(call),
                "customer_get"         => await GetCustomerTool(call),
                "customer_list"        => await ListCustomersTool(),
                "customer_portfolios"  => await GetCustomerPortfoliosTool(call),
                "customer_history"     => await GetCustomerHistoryTool(call),
                "customer_preferences" => await GetCustomerPreferencesTool(call),
                "customer_profile"     => await GetCustomerProfileTool(call),
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


        // ── Customer MCP Tools ───────────────────────────────────────────────

        private async Task<ActionResult<McpResponse>> GetCustomerTool(McpToolCall call)
        {
            var customerId = GetInt(call, "customer_id", 0);
            if (customerId <= 0)
                return BadRequest(new McpResponse { Success = false, Message = "Valid customer_id is required." });

            var customer = await _customerService.GetCustomerAsync(customerId);
            if (customer == null)
                return NotFound(new McpResponse { Success = false, Message = $"Customer {customerId} not found." });

            return Ok(McpOk(customer));
        }

        private async Task<ActionResult<McpResponse>> ListCustomersTool()
        {
            var customers = await _customerService.GetAllCustomersAsync();
            if (customers == null)
                return StatusCode(500, new McpResponse { Success = false, Message = "Failed to retrieve customers." });

            return Ok(McpOk(customers));
        }

        private async Task<ActionResult<McpResponse>> GetCustomerPortfoliosTool(McpToolCall call)
        {
            var customerId = GetInt(call, "customer_id", 0);
            if (customerId <= 0)
                return BadRequest(new McpResponse { Success = false, Message = "Valid customer_id is required." });

            var portfolios = await _customerService.GetCustomerPortfoliosAsync(customerId);
            if (portfolios == null)
                return StatusCode(500, new McpResponse { Success = false, Message = "Failed to retrieve customer portfolios." });

            return Ok(McpOk(portfolios));
        }

        private async Task<ActionResult<McpResponse>> GetCustomerHistoryTool(McpToolCall call)
        {
            var customerId = GetInt(call, "customer_id", 0);
            if (customerId <= 0)
                return BadRequest(new McpResponse { Success = false, Message = "Valid customer_id is required." });

            var history = await _customerService.GetCustomerHistoriesAsync(customerId);
            if (history == null)
                return StatusCode(500, new McpResponse { Success = false, Message = "Failed to retrieve customer history." });

            return Ok(McpOk(history));
        }

        private async Task<ActionResult<McpResponse>> GetCustomerPreferencesTool(McpToolCall call)
        {
            var customerId = GetInt(call, "customer_id", 0);
            if (customerId <= 0)
                return BadRequest(new McpResponse { Success = false, Message = "Valid customer_id is required." });

            var preferences = await _customerService.GetCustomerPreferencesAsync(customerId);
            if (preferences == null)
                return NotFound(new McpResponse { Success = false, Message = $"Preferences for customer {customerId} not found." });

            return Ok(McpOk(preferences));
        }

        private async Task<ActionResult<McpResponse>> GetCustomerProfileTool(McpToolCall call)
        {
            var customerId = GetInt(call, "customer_id", 0);
            if (customerId <= 0)
                return BadRequest(new McpResponse { Success = false, Message = "Valid customer_id is required." });

            var profile = await _customerService.GetCustomerProfileAsync(customerId);
            if (profile == null)
                return NotFound(new McpResponse { Success = false, Message = $"Customer {customerId} not found." });

            return Ok(McpOk(profile));
        }
        private static int GetInt(McpToolCall call, string key, int defaultValue)
        {
            if (call.Parameters.TryGetValue(key, out var v) && int.TryParse(v?.ToString(), out var parsed))
                return parsed;
            return defaultValue;
        }
    }
}
