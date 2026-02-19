using Microsoft.AspNetCore.Mvc;
using FxWebApi.Models;
using FxWebApi.Services;

namespace FxWebApi.Controllers
{
    [ApiController]
    [Route("mcp")]
    public class McpController : ControllerBase
    {
        private readonly FxRateService _fxRateService;
        private readonly ILogger<McpController> _logger;

        public McpController(FxRateService fxRateService, ILogger<McpController> logger)
        {
            _fxRateService = fxRateService;
            _logger = logger;
        }

        [HttpPost("fx")]
        public ActionResult<McpResponse> ExecuteFxAction([FromBody] McpRequest request)
        {
            _logger.LogInformation($"MCP FX Action: {request.Action} - Amount: {request.Amount}");

            if (string.IsNullOrEmpty(request.Action) || request.Amount <= 0)
            {
                return BadRequest(new McpResponse
                {
                    Success = false,
                    Message = "Invalid request. Action and Amount are required."
                });
            }

            var result = request.Action.ToLower() switch
            {
                "buy" => _fxRateService.ExecuteTransaction("Buy", request.Amount),
                "sell" => _fxRateService.ExecuteTransaction("Sell", request.Amount),
                _ => new FxTransactionResult
                {
                    Success = false,
                    Message = $"Unknown action: {request.Action}"
                }
            };

            return Ok(new McpResponse
            {
                Success = result.Success,
                Message = result.Message,
                Data = result.Transaction
            });
        }

        [HttpGet("status")]
        public ActionResult<McpResponse> GetStatus()
        {
            var rate = _fxRateService.GetCurrentRate();
            return Ok(new McpResponse
            {
                Success = true,
                Message = "FX Service is running",
                Data = rate
            });
        }
    }
}
