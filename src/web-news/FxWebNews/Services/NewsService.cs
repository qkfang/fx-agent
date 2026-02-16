using FxWebNews.Models;
using System.Text.Json;

namespace FxWebNews.Services
{
    public class NewsService
    {
        private readonly IWebHostEnvironment _env;
        private readonly string _newsFilePath;
        private List<NewsArticle> _newsArticles = new();

        public NewsService(IWebHostEnvironment env)
        {
            _env = env;
            _newsFilePath = Path.Combine(_env.ContentRootPath, "Data", "news.json");
            LoadNews();
        }

        private void LoadNews()
        {
            if (File.Exists(_newsFilePath))
            {
                var json = File.ReadAllText(_newsFilePath);
                _newsArticles = JsonSerializer.Deserialize<List<NewsArticle>>(json) ?? new List<NewsArticle>();
            }
        }

        private void SaveNews()
        {
            var json = JsonSerializer.Serialize(_newsArticles, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_newsFilePath, json);
        }

        public List<NewsArticle> GetAllNews()
        {
            return _newsArticles.OrderByDescending(n => n.PublishedDate).ToList();
        }

        public NewsArticle? GetNewsById(int id)
        {
            return _newsArticles.FirstOrDefault(n => n.Id == id);
        }

        public void AddNews(NewsArticle article)
        {
            article.Id = _newsArticles.Any() ? _newsArticles.Max(n => n.Id) + 1 : 1;
            article.PublishedDate = DateTime.UtcNow;
            _newsArticles.Add(article);
            SaveNews();
        }

        public bool UpdateNews(NewsArticle article)
        {
            var existing = _newsArticles.FirstOrDefault(n => n.Id == article.Id);
            if (existing == null) return false;

            existing.Title = article.Title;
            existing.Content = article.Content;
            existing.Type = article.Type;
            existing.Author = article.Author;
            SaveNews();
            return true;
        }

        public bool DeleteNews(int id)
        {
            var article = _newsArticles.FirstOrDefault(n => n.Id == id);
            if (article == null) return false;

            _newsArticles.Remove(article);
            SaveNews();
            return true;
        }
    }
}
