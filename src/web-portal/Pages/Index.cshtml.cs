using FxWebPortal.Models;
using FxWebPortal.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FxWebPortal.Pages;

public class IndexModel : PageModel
{
    private readonly ArticleService _articles;

    public IndexModel(ArticleService articles)
    {
        _articles = articles;
    }

    public List<ResearchArticle> Articles { get; set; } = new();
    public string SelectedCategory { get; set; } = "All";

    public void OnGet(string? category)
    {
        SelectedCategory = string.IsNullOrEmpty(category) ? "All" : category;
        Articles = _articles.GetPublished(SelectedCategory);
    }
}
