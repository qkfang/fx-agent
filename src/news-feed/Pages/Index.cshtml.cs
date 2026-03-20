using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FxWebNews.Models;
using FxWebNews.Services;

namespace FxWebNews.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly NewsService _newsService;
    private readonly NewsAggregatorService _aggregatorService;

    public List<NewsArticle> News { get; set; } = new();
    public string ActiveSource { get; set; } = string.Empty;

    public IndexModel(ILogger<IndexModel> logger, NewsService newsService, NewsAggregatorService aggregatorService)
    {
        _logger = logger;
        _newsService = newsService;
        _aggregatorService = aggregatorService;
    }

    public async Task OnGetAsync(string? source)
    {
        ActiveSource = source?.ToLowerInvariant() ?? string.Empty;

        News = ActiveSource switch
        {
            "bloomberg" => await _aggregatorService.GetBloombergNewsAsync(),
            "morningstar" => await _aggregatorService.GetMorningstarNewsAsync(),
            _ => _newsService.GetAllNews().Where(n => n.IsPublished && n.Source == "FX News Centre").ToList()
        };

        ViewData["ActiveSource"] = ActiveSource;
    }
}
