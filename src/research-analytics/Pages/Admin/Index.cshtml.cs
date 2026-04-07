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
    private readonly DraftService _drafts;
    private readonly TrackingService _tracking;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public IndexModel(ArticleService articles, DraftService drafts, TrackingService tracking,
        IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _articles = articles;
        _drafts = drafts;
        _tracking = tracking;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public List<ResearchArticle> Articles { get; set; } = new();
    public List<ResearchDraft> Drafts { get; set; } = new();
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

    public async Task<IActionResult> OnPostPublishDraft(int id)
    {
        var redirect = RequireAdmin();
        if (redirect != null) return redirect;

        var draft = _drafts.GetById(id);
        if (draft == null)
        {
            Message = "Draft not found.";
            LoadData();
            return Page();
        }

        var article = new ResearchArticle
        {
            Title = draft.Title,
            Content = draft.Content,
            Author = draft.Author,
            Category = draft.Category,
            Tags = draft.Tags,
            Status = "Published",
            PublishedDate = DateTime.UtcNow,
            Sentiment = "Neutral"
        };

        var created = _articles.Add(article);
        _drafts.Delete(id);

        // Invoke the research agent with the published article
        var agentUrl = _configuration["FoundryAgent:EndpointUrl"];
        if (!string.IsNullOrWhiteSpace(agentUrl))
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var prompt = $"A new research article has been published. Title: {created.Title}. Content: {created.Content}";
                var payload = JsonSerializer.Serialize(new { message = prompt });
                await client.PostAsync($"{agentUrl}/suggestion",
                    new StringContent(payload, Encoding.UTF8, "application/json"));
            }
            catch (Exception ex)
            {
                Message = $"Draft published but agent notification failed: {ex.Message}";
                LoadData();
                return Page();
            }
        }

        Message = $"Draft '{created.Title}' published and suggestion agent notified.";
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

        var brokerUrl = _configuration["CrmBrokerApi:EndpointUrl"];
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
                Message = $"Failed to reach broker CRM: {ex.Message}. Check the CrmBrokerApi:EndpointUrl configuration.";
            }
        }
        else
        {
            Message = "Broker notification URL is not configured (CrmBrokerApi:EndpointUrl).";
        }

        LoadData();
        return Page();
    }

    private void LoadData()
    {
        Articles = _articles.GetAll();
        Drafts = _drafts.GetAll();
        Visitors = _tracking.GetAll();
        Leads = _tracking.GetLeads();
        UniqueSessions = _tracking.GetUniqueSessionCount();
        AvgTimeSpent = _tracking.GetAvgTimeSpent();
    }
}
