using System.Xml.Linq;
using FxWebNews.Models;

namespace FxWebNews.Services
{
    public class NewsAggregatorService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<NewsAggregatorService> _logger;
        private readonly NewsService _newsService;

        private const string BloombergRssUrl = "https://feeds.bloomberg.com/markets/news.rss";
        private const string MorningstarRssUrl = "https://www.morningstar.com/feeds/all-articles.xml";

        public NewsAggregatorService(
            IHttpClientFactory httpClientFactory,
            ILogger<NewsAggregatorService> logger,
            NewsService newsService)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _newsService = newsService;
        }

        public async Task<List<NewsArticle>> GetBloombergNewsAsync()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("aggregator");
                var response = await client.GetStringAsync(BloombergRssUrl);
                var parsed = ParseRss(response, "Bloomberg", "Bloomberg Markets");
                if (parsed.Count > 0)
                    return parsed;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Bloomberg RSS feed unavailable ({Message}), using fallback data.", ex.Message);
            }

            return _newsService.GetAllNews()
                .Where(n => n.Source == "Bloomberg" && n.IsPublished)
                .ToList();
        }

        public async Task<List<NewsArticle>> GetMorningstarNewsAsync()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("aggregator");
                var response = await client.GetStringAsync(MorningstarRssUrl);
                var parsed = ParseRss(response, "Morningstar", "Morningstar Research");
                if (parsed.Count > 0)
                    return parsed;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Morningstar RSS feed unavailable ({Message}), using fallback data.", ex.Message);
            }

            return _newsService.GetAllNews()
                .Where(n => n.Source == "Morningstar" && n.IsPublished)
                .ToList();
        }

        private static List<NewsArticle> ParseRss(string rssXml, string source, string author)
        {
            var articles = new List<NewsArticle>();
            try
            {
                var doc = XDocument.Parse(rssXml);

                var items = doc.Descendants("item").Take(10);
                int idOffset = source == "Bloomberg" ? -1000 : -2000;
                int idx = 0;
                foreach (var item in items)
                {
                    var title = item.Element("title")?.Value ?? string.Empty;
                    var description = item.Element("description")?.Value ?? string.Empty;
                    var pubDateStr = item.Element("pubDate")?.Value;
                    var publishedDate = DateTime.TryParse(pubDateStr, out var pd) ? pd : DateTime.UtcNow;

                    // Strip HTML tags from description
                    description = System.Text.RegularExpressions.Regex.Replace(description, "<[^>]+>", string.Empty).Trim();

                    var summary = description.Length > 160 ? description[..157] + "…" : description;
                    var content = description.Length > 0 ? description : title;

                    articles.Add(new NewsArticle
                    {
                        Id = idOffset - idx++,
                        Title = title,
                        Summary = summary,
                        Content = content,
                        Type = "Bad",
                        Category = "Macro",
                        PublishedDate = publishedDate,
                        Author = author,
                        IsPublished = true,
                        PublishedAt = publishedDate,
                        Source = source
                    });
                }
            }
            catch (Exception ex)
            {
                // Return empty list; caller falls back to mock data and logs the warning
                _ = ex;
            }
            return articles;
        }
    }
}
