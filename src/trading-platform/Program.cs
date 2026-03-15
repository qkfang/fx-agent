using FxWebUI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<FxDataService>();

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

// GET /api/portfolio – return the fund summary
app.MapGet("/api/portfolio", (FxDataService svc) =>
    Results.Ok(svc.GetFundSummary()));

// GET /api/transactions – return the transaction history
app.MapGet("/api/transactions", (FxDataService svc) =>
    Results.Ok(svc.GetTransactions()));

app.Run();
