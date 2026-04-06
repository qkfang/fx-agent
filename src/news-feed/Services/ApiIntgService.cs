using FxWebNews.Models;
using System.Text.Json;

namespace FxWebNews.Services;

public class ApiIntgService
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly IConfiguration _config;
    private readonly ILogger<ApiIntgService> _logger;

    private static readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);

    public ApiIntgService(IHttpClientFactory httpFactory, IConfiguration config, ILogger<ApiIntgService> logger)
    {
        _httpFactory = httpFactory;
        _config = config;
        _logger = logger;
    }

    public async Task<List<NewsArticle>> GetTraderNewsFeedsAsync()
    {
        var baseUrl = _config["ApiIntg:BaseUrl"];
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            _logger.LogWarning("ApiIntg:BaseUrl is not configured – skipping trader news feeds.");
            return [];
        }

        try
        {
            var client = _httpFactory.CreateClient("api-intg");
            var response = await client.GetAsync($"{baseUrl.TrimEnd('/')}/api/tradernewsfeeds");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("api-intg returned {Status} for trader news feeds.", (int)response.StatusCode);
                return [];
            }

            var body = await response.Content.ReadAsStringAsync();
            var feeds = JsonSerializer.Deserialize<List<TraderNewsFeedDto>>(body, _json) ?? [];

            return feeds.Select(f => new NewsArticle
            {
                Id = f.Id,
                Title = f.Headline,
                Summary = f.Summary,
                Content = f.Summary,
                Type = f.Sentiment == "Bullish" ? "Good" : f.Sentiment == "Bearish" ? "Bad" : "Good",
                Category = f.Category,
                Author = f.Source,
                PublishedDate = f.PublishedAt,
                PublishedAt = f.PublishedAt,
                IsPublished = true,
                Source = "Trader Feeds"
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch trader news feeds from api-intg.");
            return [];
        }
    }

    private sealed class TraderNewsFeedDto
    {
        public int Id { get; set; }
        public string Headline { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Sentiment { get; set; } = "Neutral";
        public string Summary { get; set; } = string.Empty;
        public DateTime PublishedAt { get; set; }
    }
}
