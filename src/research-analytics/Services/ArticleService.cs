using FxWebPortal.Models;
using System.Text.Json;

namespace FxWebPortal.Services;

public class ArticleService
{
    private readonly HttpClient _http;
    private readonly string _baseUrl;
    private readonly ILogger<ArticleService> _logger;
    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public ArticleService(HttpClient http, IConfiguration config, ILogger<ArticleService> logger)
    {
        _http = http;
        _baseUrl = config["IntegrationApi:BaseUrl"]?.TrimEnd('/') ?? "http://localhost:5005";
        _logger = logger;
    }

    public List<ResearchArticle> GetAll()
    {
        try
        {
            var response = _http.GetAsync($"{_baseUrl}/api/researcharticles").Result;
            response.EnsureSuccessStatusCode();
            var json = response.Content.ReadAsStringAsync().Result;
            return JsonSerializer.Deserialize<List<ResearchArticle>>(json, _jsonOptions)?
                .OrderByDescending(a => a.PublishedDate).ToList() ?? new();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch articles from integration API");
            return new();
        }
    }

    public List<ResearchArticle> GetPublished(string? category = null)
    {
        var all = GetAll();
        var q = all.Where(a => a.Status == "Published");
        if (!string.IsNullOrEmpty(category) && category != "All")
            q = q.Where(a => a.Category == category);
        return q.OrderByDescending(a => a.PublishedDate).ToList();
    }

    public ResearchArticle? GetById(int id)
    {
        try
        {
            var response = _http.GetAsync($"{_baseUrl}/api/researcharticles/{id}").Result;
            if (!response.IsSuccessStatusCode) return null;
            var json = response.Content.ReadAsStringAsync().Result;
            return JsonSerializer.Deserialize<ResearchArticle>(json, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch article {Id} from integration API", id);
            return null;
        }
    }

    public ResearchArticle Add(ResearchArticle article)
    {
        try
        {
            article.PublishedDate = DateTime.UtcNow;
            var json = JsonSerializer.Serialize(article);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var response = _http.PostAsync($"{_baseUrl}/api/researcharticles", content).Result;
            response.EnsureSuccessStatusCode();
            var resultJson = response.Content.ReadAsStringAsync().Result;
            return JsonSerializer.Deserialize<ResearchArticle>(resultJson, _jsonOptions) ?? article;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to add article to integration API");
            return article;
        }
    }

    public bool Update(ResearchArticle article)
    {
        try
        {
            var json = JsonSerializer.Serialize(article);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var response = _http.PutAsync($"{_baseUrl}/api/researcharticles/{article.Id}", content).Result;
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to update article {Id} in integration API", article.Id);
            return false;
        }
    }

    public bool Publish(int id)
    {
        var article = GetById(id);
        if (article == null) return false;
        article.Status = "Published";
        article.PublishedDate = DateTime.UtcNow;
        return Update(article);
    }

    public bool Unpublish(int id)
    {
        var article = GetById(id);
        if (article == null) return false;
        article.Status = "Draft";
        return Update(article);
    }

    public bool Delete(int id)
    {
        try
        {
            var response = _http.DeleteAsync($"{_baseUrl}/api/researcharticles/{id}").Result;
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete article {Id} from integration API", id);
            return false;
        }
    }
}
