using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FxWebNews.Models;
using FxWebNews.Services;

namespace FxWebNews.Pages.Admin
{
    public class IndexModel : PageModel
    {
        private readonly NewsService _newsService;
        public List<NewsArticle> News { get; set; } = new();
        
        [TempData]
        public string? Message { get; set; }

        public IndexModel(NewsService newsService)
        {
            _newsService = newsService;
        }

        public void OnGet()
        {
            News = _newsService.GetAllNews();
        }

        public IActionResult OnPost(string title, string content, string type, string author)
        {
            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(content))
            {
                Message = "Title and content are required";
                return RedirectToPage();
            }

            var article = new NewsArticle
            {
                Title = title,
                Content = content,
                Type = type,
                Author = author
            };

            _newsService.AddNews(article);
            Message = "Article published successfully!";
            
            return RedirectToPage();
        }

        public IActionResult OnPostDelete(int id)
        {
            _newsService.DeleteNews(id);
            Message = "Article deleted successfully!";
            return RedirectToPage();
        }
    }
}
