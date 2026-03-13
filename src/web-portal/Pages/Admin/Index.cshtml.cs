using FxWebPortal.Models;
using FxWebPortal.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FxWebPortal.Pages.Admin;

public class IndexModel : PageModel
{
    private readonly ArticleService _articles;
    private readonly TrackingService _tracking;

    public IndexModel(ArticleService articles, TrackingService tracking)
    {
        _articles = articles;
        _tracking = tracking;
    }

    public List<ResearchArticle> Articles { get; set; } = new();
    public List<VisitorLog> Visitors { get; set; } = new();
    public List<VisitorLog> Leads { get; set; } = new();
    public string Message { get; set; } = string.Empty;
    public int TotalArticles => Articles.Count;
    public int PublishedCount => Articles.Count(a => a.Status == "Published");
    public int TotalVisits => Visitors.Count;
    public int TotalLeads => Leads.Count;
    public int UniqueSessions { get; set; }
    public double AvgTimeSpent { get; set; }

    private IActionResult? RequireAdmin()
    {
        if (HttpContext.Session.GetString("AdminAuth") != "true")
            return RedirectToPage("/Admin/Login");
        return null;
    }

    public IActionResult OnGet()
    {
        var redirect = RequireAdmin();
        if (redirect != null) return redirect;
        LoadData();
        return Page();
    }

    public IActionResult OnPostPublish(int id)
    {
        var redirect = RequireAdmin();
        if (redirect != null) return redirect;
        _articles.Publish(id);
        Message = "Article published successfully.";
        LoadData();
        return Page();
    }

    public IActionResult OnPostUnpublish(int id)
    {
        var redirect = RequireAdmin();
        if (redirect != null) return redirect;
        _articles.Unpublish(id);
        Message = "Article moved back to draft.";
        LoadData();
        return Page();
    }

    public IActionResult OnPostDelete(int id)
    {
        var redirect = RequireAdmin();
        if (redirect != null) return redirect;
        _articles.Delete(id);
        Message = "Article deleted.";
        LoadData();
        return Page();
    }

    private void LoadData()
    {
        Articles = _articles.GetAll();
        Visitors = _tracking.GetAll();
        Leads = _tracking.GetLeads();
        UniqueSessions = _tracking.GetUniqueSessionCount();
        AvgTimeSpent = _tracking.GetAvgTimeSpent();
    }
}
