using Microsoft.AspNetCore.Mvc;
using FxWebUI.Models;
using FxWebUI.Services;

namespace FxWebUI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionsController : ControllerBase
    {
        private readonly FxDataService _dataService;
        private readonly ILogger<TransactionsController> _logger;

        public TransactionsController(FxDataService dataService, ILogger<TransactionsController> logger)
        {
            _dataService = dataService;
            _logger = logger;
        }

        [HttpGet]
        public ActionResult<List<Transaction>> GetTransactions()
        {
            return Ok(_dataService.GetTransactions());
        }

        [HttpPost]
        public ActionResult<Transaction> AddTransaction([FromBody] Transaction transaction)
        {
            _logger.LogInformation("Received transaction: {Type} {Amount} {Pair} @ {Rate}",
                transaction.Type, transaction.Amount, transaction.CurrencyPair, transaction.Rate);
            
            var result = _dataService.AddTransaction(transaction);
            return Ok(result);
        }

        [HttpGet("summary")]
        public ActionResult<FundSummary> GetSummary()
        {
            return Ok(_dataService.GetFundSummary());
        }
    }
}
