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

        /// <summary>Get current mid-price AUD/USD rate.</summary>
        [HttpGet("rate")]
        public ActionResult<FxRate> GetRate()
        {
            return Ok(_fxRateService.GetCurrentRate());
        }

        /// <summary>Get current bid/ask quote with spread.</summary>
        [HttpGet("quote")]
        public ActionResult<FxQuote> GetQuote()
        {
            return Ok(_fxRateService.GetCurrentQuote());
        }

        /// <summary>Get OHLC candle history.</summary>
        [HttpGet("history")]
        public ActionResult<List<OhlcCandle>> GetHistory([FromQuery] int bars = 100)
        {
            bars = Math.Clamp(bars, 1, 500);
            return Ok(_fxRateService.GetHistory(bars));
        }

        /// <summary>Get live order book / market depth.</summary>
        [HttpGet("orderbook")]
        public ActionResult<OrderBook> GetOrderBook()
        {
            return Ok(_fxRateService.GetOrderBook());
        }

        /// <summary>Get market status: trend, volatility, day stats.</summary>
        [HttpGet("status")]
        public ActionResult<MarketStatus> GetStatus()
        {
            return Ok(_fxRateService.GetMarketStatus());
        }

        /// <summary>Get transaction history.</summary>
        [HttpGet("transactions")]
        public ActionResult<List<TransactionRecord>> GetTransactions([FromQuery] int limit = 50)
        {
            return Ok(_fxRateService.GetTransactions(limit));
        }

        /// <summary>Set market trend direction (up/down/neutral) and strength 0-100.</summary>
        [HttpPost("trend")]
        public ActionResult SetTrend([FromBody] TrendRequest request)
        {
            _fxRateService.SetTrend(request.Direction, request.Strength);
            _logger.LogInformation("Trend set: {Dir} @ strength {Str}", request.Direction, request.Strength);
            return Ok(new { success = true, message = $"Trend set to {request.Direction} (strength {request.Strength}%)" });
        }

        /// <summary>Execute a buy order (market buy at ask).</summary>
        [HttpPost("buy")]
        public ActionResult<FxTransactionResult> Buy([FromBody] FxTransaction transaction)
        {
            _logger.LogInformation("Buy {Amount} {Pair}", transaction.Amount, transaction.CurrencyPair);
            var result = _fxRateService.ExecuteTransaction("Buy", transaction.Amount, "API");
            return Ok(result);
        }

        /// <summary>Execute a sell order (market sell at bid).</summary>
        [HttpPost("sell")]
        public ActionResult<FxTransactionResult> Sell([FromBody] FxTransaction transaction)
        {
            _logger.LogInformation("Sell {Amount} {Pair}", transaction.Amount, transaction.CurrencyPair);
            var result = _fxRateService.ExecuteTransaction("Sell", transaction.Amount, "API");
            return Ok(result);
        }
    }
}
