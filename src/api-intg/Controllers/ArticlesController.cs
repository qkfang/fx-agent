using FxIntegrationApi.Data;
using FxIntegrationApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace FxIntegrationApi.Controllers;

[ApiController]
[Route("api/articles")]
public class ArticlesController : ControllerBase
{
    private readonly FxDbContext _db;
    private readonly ILogger<ArticlesController> _logger;

    public ArticlesController(FxDbContext db, ILogger<ArticlesController> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// Receives a published article from the news-feed app and stores it as a ResearchArticle.
    /// </summary>
    [HttpPost("receive")]
    public async Task<IActionResult> Receive([FromBody] IncomingArticle article)
    {
        var research = new ResearchArticle
        {
            Title = article.Title,
            Summary = article.Summary ?? string.Empty,
            Content = article.Content ?? string.Empty,
            Category = article.Category ?? string.Empty,
            Author = article.Author ?? "FX News Centre",
            PublishedDate = article.PublishedAt ?? DateTime.UtcNow,
            Status = "Published",
            Tags = article.Category ?? string.Empty,
            Sentiment = article.Type == "Good" ? "Bullish" : article.Type == "Bad" ? "Bearish" : "Neutral"
        };

        _db.ResearchArticles.Add(research);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Received article from news-feed (external id {Id}, db id {DbId}).", article.Id, research.Id);
        return Ok(new { id = research.Id, message = "Article received." });
    }
}

public class IncomingArticle
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? Content { get; set; }
    public string? Type { get; set; }
    public string? Category { get; set; }
    public string? Author { get; set; }
    public DateTime? PublishedAt { get; set; }
}
