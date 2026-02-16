namespace FxWebNews.Models
{
    public class NewsArticle
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // "Good" or "Bad"
        public DateTime PublishedDate { get; set; }
        public string Author { get; set; } = "FX News Team";
    }
}
