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
app.MapPost("/api/track", async (FxWebPortal.Models.TrackingRequest req, HttpContext ctx, TrackingService svc) =>
{
    req.IpAddress = ctx.Connection.RemoteIpAddress?.ToString() ?? "";
    req.UserAgent = ctx.Request.Headers["User-Agent"].ToString();
    await svc.AddLogAsync(req);
    return Results.Ok();
});

app.MapRazorPages();

app.Run();
