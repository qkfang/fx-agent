using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FxWebNews.Models;
using FxWebNews.Services;
using System.Text.Json;

namespace FxWebNews.Pages.Admin
{
    public class IndexModel : PageModel
    {
        private readonly NewsService _newsService;
        private readonly EventHubPublishService _eventHubService;
        private readonly IWebHostEnvironment _environment;
        public List<NewsArticle> News { get; set; } = new();
        public List<NewsExample> Examples { get; set; } = new();

        [TempData]
        public string? Message { get; set; }

        [TempData]
        public string? MessageType { get; set; }

        public IndexModel(NewsService newsService, EventHubPublishService eventHubService, IWebHostEnvironment environment)
        {
            _newsService = newsService;
            _eventHubService = eventHubService;
            _environment = environment;
        }

        public void OnGet()
        {
            News = _newsService.GetAllNews();
            LoadExamples();
        }

        private void LoadExamples()
        {
            try
            {
                var examplesPath = Path.Combine(_environment.ContentRootPath, "Data", "news-examples.json");
                if (System.IO.File.Exists(examplesPath))
                {
                    var json = System.IO.File.ReadAllText(examplesPath);
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    Examples = JsonSerializer.Deserialize<List<NewsExample>>(json, options) ?? new();
                }
            }
            catch
            {
                Examples = new();
            }
        }

        public async Task<IActionResult> OnPostAsync(string title, string summary, string content, string type, string category, string author)
        {
            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(content))
            {
                Message = "Title and content are required.";
                MessageType = "danger";
                return RedirectToPage();
            }

            var article = new NewsArticle
            {
                Title = title,
                Summary = summary ?? string.Empty,
                Content = content,
                Type = type,
                Category = category ?? "FX",
                Author = string.IsNullOrWhiteSpace(author) ? "FX News Team" : author,
                IsPublished = true,
                PublishedAt = DateTime.UtcNow
            };

            _newsService.AddNews(article);

            var (success, fabricMessage, _) = await _eventHubService.PublishBatchAsync(new List<NewsArticle> { article });

            if (success)
            {
                Message = $"Article \"{title}\" published and sent to Fabric. {fabricMessage}";
                MessageType = "success";
            }
            else
            {
                Message = $"Article \"{title}\" published locally, but Fabric send failed: {fabricMessage}";
                MessageType = "warning";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRetryFabricAsync(int id)
        {
            var article = _newsService.GetNewsById(id);
            if (article == null)
            {
                Message = "Article not found.";
                MessageType = "danger";
                return RedirectToPage();
            }

            var (success, fabricMessage, _) = await _eventHubService.PublishBatchAsync(new List<NewsArticle> { article });

            if (success)
            {
                Message = $"Article \"{article.Title}\" sent to Fabric successfully. {fabricMessage}";
                MessageType = "success";
            }
            else
            {
                Message = $"Retry failed for \"{article.Title}\": {fabricMessage}";
                MessageType = "danger";
            }

            return RedirectToPage();
        }

        public IActionResult OnPostDelete(int id)
        {
            _newsService.DeleteNews(id);
            Message = "Article deleted successfully.";
            MessageType = "success";
            return RedirectToPage();
        }
    }
}

