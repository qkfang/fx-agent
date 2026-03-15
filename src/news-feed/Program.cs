using FxWebNews.Models;
using FxWebNews.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddSingleton<NewsService>();
builder.Services.AddHttpClient<NewsPublishService>();

// Add CORS for local development (fx-agent Python service needs cross-origin access)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors();
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();

// ── REST API endpoints (consumed by fx-agent simulation) ─────────────────────

// GET /api/news – list all news articles
app.MapGet("/api/news", (NewsService svc) =>
    Results.Ok(svc.GetAllNews()));

// POST /api/news – create and return a new news article
app.MapPost("/api/news", (NewsArticle article, NewsService svc) =>
{
    svc.AddNews(article);
    return Results.Ok(article);
});

app.Run();
