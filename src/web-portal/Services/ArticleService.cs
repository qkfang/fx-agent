using FxWebPortal.Models;
using System.Text.Json;

namespace FxWebPortal.Services;

public class ArticleService
{
    private readonly string _filePath;
    private List<ResearchArticle> _articles = new();
    private readonly object _lock = new();

    public ArticleService(IWebHostEnvironment env)
    {
        _filePath = Path.Combine(env.ContentRootPath, "Data", "articles.json");
        Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
        Load();
    }

    private void Load()
    {
        lock (_lock)
        if (File.Exists(_filePath))
        {
            var json = File.ReadAllText(_filePath);
            _articles = JsonSerializer.Deserialize<List<ResearchArticle>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
        }
    }

    private void Save()
    {
        var json = JsonSerializer.Serialize(_articles, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_filePath, json);
    }

    public List<ResearchArticle> GetAll()
    {
        lock (_lock) return _articles.OrderByDescending(a => a.PublishedDate).ToList();
    }

    public List<ResearchArticle> GetPublished(string? category = null)
    {
        lock (_lock)
        {
            var q = _articles.Where(a => a.Status == "Published");
            if (!string.IsNullOrEmpty(category) && category != "All")
                q = q.Where(a => a.Category == category);
            return q.OrderByDescending(a => a.PublishedDate).ToList();
        }
    }

    public ResearchArticle? GetById(int id)
    {
        lock (_lock) return _articles.FirstOrDefault(a => a.Id == id);
    }

    public ResearchArticle Add(ResearchArticle article)
    {
        lock (_lock)
        {
            article.Id = _articles.Any() ? _articles.Max(a => a.Id) + 1 : 1;
            article.PublishedDate = DateTime.UtcNow;
            _articles.Add(article);
            Save();
            return article;
        }
    }

    public bool Update(ResearchArticle article)
    {
        lock (_lock)
        {
            var existing = _articles.FirstOrDefault(a => a.Id == article.Id);
            if (existing == null) return false;
            existing.Title = article.Title;
            existing.Summary = article.Summary;
            existing.Content = article.Content;
            existing.Category = article.Category;
            existing.Author = article.Author;
            existing.Tags = article.Tags;
            existing.Sentiment = article.Sentiment;
            existing.Status = article.Status;
            Save();
            return true;
        }
    }

    public bool Publish(int id)
    {
        lock (_lock)
        {
            var article = _articles.FirstOrDefault(a => a.Id == id);
            if (article == null) return false;
            article.Status = "Published";
            article.PublishedDate = DateTime.UtcNow;
            Save();
            return true;
        }
    }

    public bool Unpublish(int id)
    {
        lock (_lock)
        {
            var article = _articles.FirstOrDefault(a => a.Id == id);
            if (article == null) return false;
            article.Status = "Draft";
            Save();
            return true;
        }
    }

    public bool Delete(int id)
    {
        lock (_lock)
        {
            var article = _articles.FirstOrDefault(a => a.Id == id);
            if (article == null) return false;
            _articles.Remove(article);
            Save();
            return true;
        }
    }
}
