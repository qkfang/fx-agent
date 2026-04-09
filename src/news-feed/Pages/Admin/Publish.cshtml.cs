using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FxWebNews.Models;
using FxWebNews.Services;

namespace FxWebNews.Pages.Admin
{
    public class PublishModel : PageModel
    {
        private readonly NewsService _newsService;
        private readonly NewsPublishService _publishService;

        public NewsArticle? Article { get; set; }

        [TempData]
        public string? Message { get; set; }

        [TempData]
        public string? MessageType { get; set; }

        public PublishModel(NewsService newsService, NewsPublishService publishService)
        {
            _newsService = newsService;
            _publishService = publishService;
        }

        public IActionResult OnGet(int id)
        {
            Article = _newsService.GetNewsById(id);
            if (Article == null)
            {
                return RedirectToPage("/Admin/Index");
            }
            return Page();
        }

        public IActionResult OnPostDelete(int id)
        {
            _newsService.DeleteNews(id);
            return RedirectToPage("/Admin/Index");
        }
    }
}
