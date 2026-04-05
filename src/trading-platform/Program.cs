using FxWebUI.Models;
using FxWebUI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<FxDataService>();

builder.Services.AddMcpServer()
    .WithHttpTransport(options => { options.Stateless = true; })
    .WithToolsFromAssembly();

// Add CORS
builder.Services.AddCors();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Add CORS for MCP access
app.UseCors(policy => policy
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// Receive a settled trade from Broker Back-Office
app.MapPost("/api/trades", (Transaction transaction, FxDataService fxData) =>
{
    var settled = fxData.AddTransaction(transaction);
    return Results.Ok(new { settled = true, id = settled.Id });
});

// Expose transaction history as JSON (used by FX Agent)
app.MapGet("/api/trades", (FxDataService fxData) =>
    Results.Ok(fxData.GetTransactions()));

// Expose portfolio/fund summary (used by FX Agent)
app.MapGet("/api/portfolio", (FxDataService fxData) =>
    Results.Ok(fxData.GetFundSummary()));

app.MapControllers();
app.MapRazorPages();
app.MapMcp("/mcp");

app.Run();
