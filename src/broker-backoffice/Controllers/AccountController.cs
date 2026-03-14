using Microsoft.AspNetCore.Mvc;
using FxWebApi.Models;
using FxWebApi.Services;

namespace FxWebApi.Controllers
{
    [ApiController]
    [Route("api/accounts")]
    public class AccountController : ControllerBase
    {
        private readonly AccountService _accountService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(AccountService accountService, ILogger<AccountController> logger)
        {
            _accountService = accountService;
            _logger = logger;
        }

        [HttpGet]
        public ActionResult<List<AccountSummary>> GetAllAccounts()
        {
            var accounts = _accountService.GetAllAccounts();
            return Ok(accounts);
        }

        [HttpGet("{id}/balance")]
        public ActionResult<BalanceSheet> GetBalanceSheet(int id)
        {
            var sheet = _accountService.GetBalanceSheet(id);
            if (sheet == null) return NotFound();
            return Ok(sheet);
        }

        [HttpPost("{id}/buy")]
        public ActionResult<FxTransactionResult> Buy(int id, [FromBody] TradeRequest request)
        {
            _logger.LogInformation("Buy {Lots} lots for account {Id}", request.Lots, id);
            var result = _accountService.ExecuteTrade(id, "Buy", request.Lots);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("{id}/sell")]
        public ActionResult<FxTransactionResult> Sell(int id, [FromBody] TradeRequest request)
        {
            _logger.LogInformation("Sell {Lots} lots for account {Id}", request.Lots, id);
            var result = _accountService.ExecuteTrade(id, "Sell", request.Lots);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("{id}/close/{positionId}")]
        public ActionResult<FxTransactionResult> ClosePosition(int id, string positionId)
        {
            _logger.LogInformation("Close position {PositionId} for account {Id}", positionId, id);
            var result = _accountService.ClosePosition(id, positionId);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}
