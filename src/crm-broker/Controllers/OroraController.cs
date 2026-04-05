using Microsoft.AspNetCore.Mvc;
using FxWebApi.Models;
using FxWebApi.Services;

namespace FxWebApi.Controllers
{
    /// <summary>
    /// Aurora trading endpoint — called by the Research Analytics portal when a
    /// user clicks the Aurora popup to execute a trade on behalf of a customer.
    /// Executes the trade via the CRM AccountService and returns the result so the
    /// portal can record the outcome and update the CRM record.
    /// </summary>
    [ApiController]
    [Route("api/aurora")]
    public class AuroraController : ControllerBase
    {
        private readonly AccountService _accountService;
        private readonly FxRateService _fxRateService;
        private readonly ILogger<AuroraController> _logger;

        public AuroraController(AccountService accountService, FxRateService fxRateService,
            ILogger<AuroraController> logger)
        {
            _accountService = accountService;
            _fxRateService = fxRateService;
            _logger = logger;
        }

        /// <summary>
        /// Execute a trade through Aurora: buy or sell on the trading platform and
        /// record the transaction in the CRM.
        /// </summary>
        [HttpPost("trade")]
        public ActionResult<AuroraTradeResult> ExecuteTrade([FromBody] AuroraTradeRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Direction) ||
                (request.Direction != "Buy" && request.Direction != "Sell"))
            {
                return BadRequest(new AuroraTradeResult
                {
                    Success = false,
                    Message = "Direction must be 'Buy' or 'Sell'."
                });
            }

            if (request.Lots <= 0 || request.Lots > 100)
            {
                return BadRequest(new AuroraTradeResult
                {
                    Success = false,
                    Message = "Lots must be between 0.01 and 100."
                });
            }

            _logger.LogInformation("Aurora trade: {Direction} {Lots} lots of {Pair} for account {AccountId} ({Customer})",
                request.Direction, request.Lots, request.CurrencyPair, request.AccountId, request.CustomerName);

            var result = _accountService.ExecuteTrade(request.AccountId, request.Direction, request.Lots);

            var rate = _fxRateService.GetCurrentRate().Rate;

            return result.Success
                ? Ok(new AuroraTradeResult
                {
                    Success = true,
                    Message = result.Message,
                    Direction = request.Direction,
                    CurrencyPair = request.CurrencyPair,
                    Lots = request.Lots,
                    Rate = result.Transaction?.Rate ?? rate,
                    TransactionId = result.Record?.Id ?? $"ORR-{DateTime.UtcNow.Ticks}"
                })
                : BadRequest(new AuroraTradeResult
                {
                    Success = false,
                    Message = result.Message,
                    Direction = request.Direction,
                    CurrencyPair = request.CurrencyPair,
                    Lots = request.Lots,
                    Rate = rate
                });
        }

        /// <summary>Returns the list of CRM accounts available for Aurora trade assignment.</summary>
        [HttpGet("accounts")]
        public ActionResult<List<AccountSummary>> GetAccounts()
        {
            return Ok(_accountService.GetAllAccounts());
        }
    }
}
