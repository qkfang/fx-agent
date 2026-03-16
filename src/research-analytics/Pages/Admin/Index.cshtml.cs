using FxWebPortal.Models;
using FxWebPortal.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using System.Text.Json;

namespace FxWebPortal.Pages.Admin;

public class IndexModel : PageModel
{
    private readonly ArticleService _articles;
    private readonly TrackingService _tracking;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public IndexModel(ArticleService articles, TrackingService tracking,
        IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _articles = articles;
        _tracking = tracking;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
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

    public async Task<IActionResult> OnPostSendToBroker(int id)
    {
        var redirect = RequireAdmin();
        if (redirect != null) return redirect;

        var lead = _tracking.GetLeads().FirstOrDefault(l => l.Id == id);
        if (lead == null)
        {
            Message = "Lead not found.";
            LoadData();
            return Page();
        }

        var brokerUrl = _configuration["BrokerNotification:EndpointUrl"];
        if (!string.IsNullOrWhiteSpace(brokerUrl))
        {
            try
            {
                var article = lead.ArticleId.HasValue ? _articles.GetById(lead.ArticleId.Value) : null;
                var payload = new
                {
                    userName = lead.UserName,
                    userEmail = lead.UserEmail,
                    userCompany = lead.UserCompany,
                    articleId = lead.ArticleId,
                    articleTitle = article?.Title ?? string.Empty,
                    timeSpentSeconds = lead.TimeSpentSeconds,
                    sessionId = lead.SessionId
                };
                var client = _httpClientFactory.CreateClient();
                var json = JsonSerializer.Serialize(payload);
                var response = await client.PostAsync(brokerUrl,
                    new StringContent(json, Encoding.UTF8, "application/json"));
                Message = response.IsSuccessStatusCode
                    ? $"Lead for {lead.UserEmail} sent to broker successfully."
                    : $"Broker returned an error: {(int)response.StatusCode}.";
            }
            catch (Exception ex)
            {
                Message = $"Failed to reach broker backoffice: {ex.Message}. Check the BrokerNotification:EndpointUrl configuration.";
            }
        }
        else
        {
            Message = "Broker notification URL is not configured (BrokerNotification:EndpointUrl).";
        }

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
