using FxWebPortal.Models;
using FxWebPortal.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FxWebPortal.Pages;

public class ArticleModel : PageModel
{
    private readonly ArticleService _articles;

    public ArticleModel(ArticleService articles)
    {
        _articles = articles;
    }

    public ResearchArticle? Article { get; set; }
    public List<ResearchArticle> Related { get; set; } = new();

    public void OnGet(int id)
    {
        Article = _articles.GetById(id);
        if (Article == null || Article.Status != "Published") Article = null;

        if (Article != null)
        {
            Related = _articles.GetPublished(Article.Category)
                .Where(a => a.Id != Article.Id)
                .Take(4)
                .ToList();

            if (Related.Count < 2)
            {
                var others = _articles.GetPublished()
                    .Where(a => a.Id != Article.Id && a.Category != Article.Category)
                    .Take(4 - Related.Count)
                    .ToList();
                Related.AddRange(others);
            }
        }
    }
}
