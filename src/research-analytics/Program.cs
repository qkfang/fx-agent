using FxWebPortal.Models;
using FxWebPortal.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddSingleton<ArticleService>();
builder.Services.AddSingleton<TrackingService>();

// Add CORS for local development (fx-agent Python service needs cross-origin access)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

// Minimal API endpoint for visitor tracking (receives JSON beacon from tracker.js)
app.MapPost("/api/track", async (TrackingRequest req, HttpContext ctx, TrackingService svc) =>
{
    req.IpAddress = ctx.Connection.RemoteIpAddress?.ToString() ?? "";
    req.UserAgent = ctx.Request.Headers["User-Agent"].ToString();
    await svc.AddLogAsync(req);
    return Results.Ok();
});

// ── REST API endpoints (consumed by fx-agent simulation) ─────────────────────

// GET /api/articles?category=AUD/USD – list published research articles
app.MapGet("/api/articles", (string? category, ArticleService svc) =>
    Results.Ok(svc.GetPublished(category)));

// POST /api/articles – create and immediately publish a new research article
app.MapPost("/api/articles", (ResearchArticle article, ArticleService svc) =>
{
    article.Status = "Published";
    var created = svc.Add(article);
    return Results.Ok(created);
});

app.MapRazorPages();

app.Run();
