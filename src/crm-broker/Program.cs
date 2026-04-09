using FxWebApi.Data;
using FxWebApi.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { 
        Title = "FX CRM Broker API", 
        Version = "v1",
        Description = "CRM Broker API with MCP endpoints for customer information and FX trading"
    });
});

builder.Services.AddDbContext<FxDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("FxDatabase")));

builder.Services.AddHttpClient();
builder.Services.AddSingleton<FxRateService>();
builder.Services.AddSingleton<AccountService>();
builder.Services.AddScoped<CustomerService>();

// Add CORS for local development
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Warm up database connection on startup to avoid slow first request
_ = Task.Run(async () =>
{
    try
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FxDbContext>();
        await db.Customers.CountAsync();
        app.Logger.LogInformation("Database warmup completed");
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex, "Database warmup failed");
    }
});

app.Run();
