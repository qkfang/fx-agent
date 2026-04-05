namespace FxWebPortal.Models;

public class Trader
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Desk { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime JoinedAt { get; set; }

    public ICollection<TraderRecommendation> Recommendations { get; set; } = new List<TraderRecommendation>();
    public ICollection<TraderNewsFeed> NewsFeeds { get; set; } = new List<TraderNewsFeed>();
}
