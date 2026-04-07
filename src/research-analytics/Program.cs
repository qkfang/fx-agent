using FxWebPortal.Models;
using FxWebPortal.Services;
using System.Text;
using System.Text.Json;
using Azure.AI.Extensions.OpenAI;
using Azure.AI.Projects;
using Azure.Identity;
using OpenAI.Responses;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddHttpClient<ArticleService>();
builder.Services.AddHttpClient<DraftService>();
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
        var brokerUrl = config["CrmBrokerApi:EndpointUrl"];
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

app.MapPost("/api/agent/trader", async (HttpContext ctx, IConfiguration config) =>
{
    using var reader = new StreamReader(ctx.Request.Body);
    var body = await reader.ReadToEndAsync();

    var projectEndpoint = config["FoundryAgent:ProjectEndpoint"]
        ?? "https://fxag-foundry.services.ai.azure.com/api/projects/fxag-foundry-project";


    var tenantId = app.Configuration["AZURE_TENANT_ID"];
    var defaultCredential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
    {
        TenantId = tenantId
    });

    AIProjectClient aiProjectClient = new(new Uri(projectEndpoint), defaultCredential);

    var responseClient = aiProjectClient.ProjectOpenAIClient
        .GetProjectResponsesClientForAgent("fxag-trader");

    var nextOptions = new OpenAI.Responses.CreateResponseOptions
    {
        InputItems = { OpenAI.Responses.ResponseItem.CreateUserMessageItem(body) }
    };

    OpenAI.Responses.ResponseResult? result = null;

    try
    {
        while (nextOptions is not null)
        {
            result = await responseClient.CreateResponseAsync(nextOptions);
            nextOptions = null;

            foreach (var item in result.OutputItems)
            {
                if (item is OpenAI.Responses.McpToolCallApprovalRequestItem mcpCall)
                {
                    nextOptions ??= new OpenAI.Responses.CreateResponseOptions
                        { PreviousResponseId = result.Id };
                    nextOptions.InputItems.Add(
                        OpenAI.Responses.ResponseItem.CreateMcpApprovalResponseItem(mcpCall.Id, approved: true));
                }
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error calling agent: {ex}");
        return Results.Problem($"Agent call failed: {ex.Message}", statusCode: 500);
    }

    return Results.Content(result?.GetOutputText() ?? string.Empty, "text/plain");
});

app.MapRazorPages();

// API: Publish a research draft → create article, delete draft, invoke agent
app.MapPost("/api/admin/publish-draft/{id}", async (int id, DraftService drafts, ArticleService articles,
    IHttpClientFactory httpClientFactory, IConfiguration config) =>
{
    var draft = drafts.GetById(id);
    if (draft == null)
        return Results.NotFound(new { step = "load", error = "Draft not found." });

    var article = new ResearchArticle
    {
        Title = draft.Title,
        Content = draft.Content,
        Author = draft.Author,
        Category = draft.Category,
        Tags = draft.Tags,
        Status = "Published",
        PublishedDate = DateTime.UtcNow,
        Sentiment = "Neutral"
    };

    var created = articles.Add(article);
    if (created.Id == 0)
        return Results.Problem("Failed to create article.", statusCode: 500);

    var deleted = await drafts.DeleteAsync(id);
    if (!deleted)
        return Results.Problem("Article created but draft could not be deleted.", statusCode: 500);

    var agentUrl = config["FoundryAgent:EndpointUrl"];
    if (!string.IsNullOrWhiteSpace(agentUrl))
    {
        try
        {
            var client = httpClientFactory.CreateClient();
            var prompt = $"A new research article has been published. Title: {created.Title}. Content: {created.Content}";
            var payload = JsonSerializer.Serialize(new { message = prompt });
            await client.PostAsync($"{agentUrl}/suggestion",
                new StringContent(payload, Encoding.UTF8, "application/json"));
        }
        catch
        {
            return Results.Ok(new { articleId = created.Id, title = created.Title, agentNotified = false });
        }
    }

    return Results.Ok(new { articleId = created.Id, title = created.Title, agentNotified = true });
});

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

