namespace FxWebNews.Models
{
    public class NewsArticle
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // "Good" or "Bad"
        public string Category { get; set; } = "FX"; // e.g. FX, Equities, Commodities, Crypto
        public DateTime PublishedDate { get; set; }
        public string Author { get; set; } = "FX News Team";
        public bool IsPublished { get; set; } = false;
        public DateTime? PublishedAt { get; set; }
        public string Source { get; set; } = "FX News Centre"; // e.g. FX News Centre, Morningstar, Bloomberg
    }
}
