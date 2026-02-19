using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FxWebUI.Models;
using FxWebUI.Services;

namespace FxWebUI.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly FxDataService _fxDataService;

    public FxRate? CurrentRate { get; set; }
    public List<Transaction>? Transactions { get; set; }
    public FundSummary? FundSummary { get; set; }

    public IndexModel(ILogger<IndexModel> logger, FxDataService fxDataService)
    {
        _logger = logger;
        _fxDataService = fxDataService;
    }

    public async Task OnGetAsync()
    {
        CurrentRate = await _fxDataService.GetCurrentFxRate();
        Transactions = _fxDataService.GetTransactions();
        FundSummary = _fxDataService.GetFundSummary();
    }
}
