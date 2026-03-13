namespace FxWebPortal.Models;

public class TrackingRequest
{
    public string SessionId { get; set; } = string.Empty;
    public int? ArticleId { get; set; }
    public string PageUrl { get; set; } = string.Empty;
    public int TimeSpentSeconds { get; set; }
    public int ClickCount { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string UserCompany { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public string Timezone { get; set; } = string.Empty;
    public string ScreenSize { get; set; } = string.Empty;
    public string Referrer { get; set; } = string.Empty;
    // Set server-side
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
}
