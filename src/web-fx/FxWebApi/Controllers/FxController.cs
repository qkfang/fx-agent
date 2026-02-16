using Microsoft.AspNetCore.Mvc;
using FxWebApi.Models;
using FxWebApi.Services;

namespace FxWebApi.Controllers
{
    [ApiController]
    [Route("api/fx")]
    public class FxController : ControllerBase
    {
        private readonly FxRateService _fxRateService;
        private readonly ILogger<FxController> _logger;

        public FxController(FxRateService fxRateService, ILogger<FxController> logger)
        {
            _fxRateService = fxRateService;
            _logger = logger;
        }

        [HttpGet("rate")]
        public ActionResult<FxRate> GetRate()
        {
            var rate = _fxRateService.GetCurrentRate();
            return Ok(rate);
        }

        [HttpPost("buy")]
        public ActionResult<FxTransactionResult> Buy([FromBody] FxTransaction transaction)
        {
            _logger.LogInformation($"Buy transaction: {transaction.Amount} {transaction.CurrencyPair}");
            var result = _fxRateService.ExecuteTransaction("Buy", transaction.Amount);
            return Ok(result);
        }

        [HttpPost("sell")]
        public ActionResult<FxTransactionResult> Sell([FromBody] FxTransaction transaction)
        {
            _logger.LogInformation($"Sell transaction: {transaction.Amount} {transaction.CurrencyPair}");
            var result = _fxRateService.ExecuteTransaction("Sell", transaction.Amount);
            return Ok(result);
        }
    }
}
