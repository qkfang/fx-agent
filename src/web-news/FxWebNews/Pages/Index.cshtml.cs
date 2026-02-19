using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FxWebNews.Models;
using FxWebNews.Services;

namespace FxWebNews.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly NewsService _newsService;

    public List<NewsArticle> News { get; set; } = new();

    public IndexModel(ILogger<IndexModel> logger, NewsService newsService)
    {
        _logger = logger;
        _newsService = newsService;
    }

    public void OnGet()
    {
        News = _newsService.GetAllNews();
    }
}
