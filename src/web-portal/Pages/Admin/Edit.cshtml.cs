using FxWebPortal.Models;
using FxWebPortal.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FxWebPortal.Pages.Admin;

public class EditModel : PageModel
{
    private readonly ArticleService _articles;

    public EditModel(ArticleService articles)
    {
        _articles = articles;
    }

    public ResearchArticle Article { get; set; } = new() { Status = "Draft", Sentiment = "Neutral", Author = "FX Research Team" };
    public bool IsNew => Article.Id == 0;
    public string Error { get; set; } = string.Empty;

    private IActionResult? RequireAdmin()
    {
        if (HttpContext.Session.GetString("AdminAuth") != "true")
            return RedirectToPage("/Admin/Login");
        return null;
    }

    public IActionResult OnGet(int? id)
    {
        var redirect = RequireAdmin();
        if (redirect != null) return redirect;

        if (id.HasValue)
        {
            var existing = _articles.GetById(id.Value);
            if (existing == null) return NotFound();
            Article = existing;
        }
        return Page();
    }

    public IActionResult OnPost(ResearchArticle article)
    {
        var redirect = RequireAdmin();
        if (redirect != null) return redirect;

        if (!ModelState.IsValid)
        {
            Article = article;
            Error = "Please fill in all required fields.";
            return Page();
        }

        if (article.Id == 0)
        {
            _articles.Add(article);
        }
        else
        {
            _articles.Update(article);
        }

        return RedirectToPage("/Admin/Index");
    }
}
