using FxWebPortal.Models;
using FxWebPortal.Services;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddHttpClient<ArticleService>();
builder.Services.AddSingleton<TrackingService>();
builder.Services.AddHttpClient<SuggestionService>();
builder.Services.AddHttpClient<ChatService>();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<ChatKitStore>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

// Minimal API endpoint for visitor tracking (receives JSON beacon from tracker.js)
app.MapPost("/api/track", async (TrackingRequest req, HttpContext ctx, TrackingService svc,
    ArticleService articles, IHttpClientFactory httpClientFactory, IConfiguration config,
    ILoggerFactory loggerFactory) =>
{
    req.IpAddress = ctx.Connection.RemoteIpAddress?.ToString() ?? "";
    req.UserAgent = ctx.Request.Headers["User-Agent"].ToString();
    await svc.AddLogAsync(req);

    // Notify broker when a customer with an email reads an article
    if (!string.IsNullOrWhiteSpace(req.UserEmail) && req.ArticleId.HasValue)
    {
        var brokerUrl = config["BrokerNotification:EndpointUrl"];
        if (!string.IsNullOrWhiteSpace(brokerUrl))
        {
            var article = articles.GetById(req.ArticleId.Value);
            var lead = new
            {
                userName = req.UserName,
                userEmail = req.UserEmail,
                userCompany = req.UserCompany,
                articleId = req.ArticleId,
                articleTitle = article?.Title ?? string.Empty,
                timeSpentSeconds = req.TimeSpentSeconds,
                sessionId = req.SessionId
            };
            var logger = loggerFactory.CreateLogger("TrackingEndpoint");
            // Fire-and-forget – do not block the tracker response
            _ = Task.Run(async () =>
            {
                try
                {
                    var client = httpClientFactory.CreateClient();
                    var json = JsonSerializer.Serialize(lead);
                    await client.PostAsync(brokerUrl, new StringContent(json, Encoding.UTF8, "application/json"));
                }
                catch (Exception ex)
                {
                    // Mask the email to avoid PII exposure in logs.
                    var maskedEmail = req.UserEmail.Length > 3
                        ? $"{req.UserEmail[0]}***@{req.UserEmail.Split('@').LastOrDefault()}"
                        : "***";
                    logger.LogWarning(ex, "Failed to notify broker of lead for {Email}", maskedEmail);
                }
            });
        }
    }

    return Results.Ok();
});

// Receive a news article from the News Feed and auto-create a research note
app.MapPost("/api/articles/receive", (NewsIntakeRequest req, ArticleService articles) =>
{
    var sentiment = req.Type?.ToLower() switch
    {
        "bad" => "Bearish",
        "good" => "Bullish",
        _ => "Neutral"
    };

    var researchContent = $"<p><strong>News Alert:</strong> {req.Summary}</p>" +
        $"<p>{req.Content}</p>" +
        "<p><strong>Research Implication:</strong> This development may impact AUD/USD and commodity-linked currencies. " +
        "Traders should monitor safe-haven flows and risk sentiment closely.</p>";

    var article = new ResearchArticle
    {
        Title = req.Title,
        Summary = req.Summary,
        Content = researchContent,
        Category = string.IsNullOrWhiteSpace(req.Category) ? "FX" : req.Category,
        Author = string.IsNullOrWhiteSpace(req.Author) ? "Research Analytics Team" : req.Author,
        Sentiment = sentiment,
        Status = "Published",
        Tags = $"{req.Category},NewsAlert,{(sentiment == "Bearish" ? "Risk" : sentiment == "Bullish" ? "Opportunity" : "Watch")}"
    };

    var created = articles.Add(article);
    return Results.Ok(new { received = true, articleId = created.Id, title = created.Title });
});

app.MapRazorPages();

// API: ChatKit protocol endpoint (openai/chatkit-js self-hosted backend)
app.MapPost("/chatkit", (HttpContext ctx, ChatKitStore store, ChatService chatService) =>
    ChatKitHandler.HandleAsync(ctx, store, chatService));

// API: List all customer suggestions
app.MapGet("/api/suggestions", (SuggestionService suggestions) =>
{
    return Results.Ok(suggestions.GetAll());
});

// API: Advanced research chat with references and temperature control
app.MapPost("/api/chat/ask", async (HttpContext ctx, ChatService chatService, ArticleService articleService) =>
{
    using var reader = new StreamReader(ctx.Request.Body);
    var body = await reader.ReadToEndAsync();
    using var doc = JsonDocument.Parse(body);
    var root = doc.RootElement;

    var message = root.TryGetProperty("message", out var m) ? m.GetString() ?? "" : "";
    var temperature = root.TryGetProperty("temperature", out var t) ? t.GetDouble() : 0.7;

    var agentResponse = await chatService.SendMessageWithOptionsAsync(message, temperature);

    var keywords = message.ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries)
        .Where(w => w.Length > 3)
        .ToHashSet();

    var references = articleService.GetPublished()
        .Where(a =>
        {
            var searchable = $"{a.Title} {a.Summary} {a.Tags} {a.Category}".ToLowerInvariant();
            return keywords.Any(k => searchable.Contains(k));
        })
        .OrderByDescending(a => a.PublishedDate)
        .Take(5)
        .Select(a => new
        {
            a.Id,
            a.Title,
            a.Summary,
            a.Category,
            a.Sentiment,
            a.Author,
            PublishedDate = a.PublishedDate.ToString("MMM dd, yyyy"),
            Url = $"/Article?id={a.Id}"
        })
        .ToList();

    return Results.Ok(new { response = agentResponse, references });
});

app.Run();

// ── Minimal model for incoming news articles ─────────────────────────────────

/// <summary>Payload sent by the News Feed when publishing an article.</summary>
public record NewsIntakeRequest(
    int Id,
    string Title,
    string Summary,
    string Content,
    string Type,
    string Category,
    string Author,
    DateTime? PublishedAt
);

public record ChatRequest(string Message, List<ChatTurn>? History);

