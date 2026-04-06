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
        private readonly IWebHostEnvironment _environment;
        public List<NewsArticle> News { get; set; } = new();
        public List<NewsExample> Examples { get; set; } = new();

        [TempData]
        public string? Message { get; set; }

        [TempData]
        public string? MessageType { get; set; }

        public IndexModel(NewsService newsService, IWebHostEnvironment environment)
        {
            _newsService = newsService;
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

        public IActionResult OnPost(string title, string summary, string content, string type, string category, string author)
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
                IsPublished = false
            };

            _newsService.AddNews(article);
            Message = $"Article \"{title}\" saved as draft. Go to the article to publish it.";
            MessageType = "info";

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

