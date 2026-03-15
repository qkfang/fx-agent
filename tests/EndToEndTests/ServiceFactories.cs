using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

// ── Shared plumbing ──────────────────────────────────────────────────────────

/// <summary>
/// Replaces the default IHttpClientFactory so that every outbound HTTP call
/// made by the service under test is routed through <paramref name="handler"/>
/// – which is a test-server handler pointing at a downstream service's
/// in-memory test server.
/// </summary>
internal sealed class RoutingHttpClientFactory : IHttpClientFactory
{
    private readonly HttpMessageHandler _handler;

    internal RoutingHttpClientFactory(HttpMessageHandler handler) => _handler = handler;

    /// <summary>Returns an HttpClient backed by the injected test-server handler.</summary>
    public HttpClient CreateClient(string name) =>
        new HttpClient(_handler, disposeHandler: false);
}

// ── Trading Platform factory ─────────────────────────────────────────────────

/// <summary>
/// WebApplicationFactory for the <c>trading-platform</c> service.
/// Uses a temp content root so data files are isolated per test run.
/// No outbound HTTP calls to other test services.
/// </summary>
public sealed class TradingPlatformFactory : WebApplicationFactory<FxWebUI.Services.FxDataService>
{
    public string TempRoot { get; } =
        Path.Combine(Path.GetTempPath(), $"fxtest_tp_{Guid.NewGuid():N}");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        Directory.CreateDirectory(Path.Combine(TempRoot, "Data"));
        File.WriteAllText(Path.Combine(TempRoot, "Data", "transactions.json"), "[]");
        File.WriteAllText(
            Path.Combine(TempRoot, "Data", "fund.json"),
            """{"totalBalance":100000.00,"audBalance":50000.00,"usdBalance":35000.00,"totalProfitLoss":0.00}""");

        builder.UseContentRoot(TempRoot);
        builder.UseEnvironment("Development");
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing && Directory.Exists(TempRoot))
            // Best-effort cleanup of the temp content root created for test isolation.
            // A failure here does not invalidate test results.
            try { Directory.Delete(TempRoot, recursive: true); } catch { /* intentional: temp-dir cleanup is non-critical */ }
    }
}

// ── Broker Back-Office factory ───────────────────────────────────────────────

/// <summary>
/// WebApplicationFactory for the <c>broker-backoffice</c> service.
/// Outbound calls to the Trading Platform are routed through
/// <paramref name="tradingPlatformHandler"/> (from <see cref="TradingPlatformFactory"/>).
/// </summary>
public sealed class BrokerWebAppFactory : WebApplicationFactory<FxWebApi.Services.FxRateService>
{
    private readonly HttpMessageHandler _tradingPlatformHandler;

    public BrokerWebAppFactory(HttpMessageHandler tradingPlatformHandler)
        => _tradingPlatformHandler = tradingPlatformHandler;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        // The URL value does not matter – the routing handler intercepts all calls.
        builder.UseSetting("TradingPlatformUrl", "http://localhost");

        builder.ConfigureTestServices(services =>
        {
            // Route all outbound HTTP from AccountService to the trading-platform test server.
            services.AddSingleton<IHttpClientFactory>(
                new RoutingHttpClientFactory(_tradingPlatformHandler));
        });
    }
}

// ── Research Analytics factory ───────────────────────────────────────────────

/// <summary>
/// WebApplicationFactory for the <c>research-analytics</c> service.
/// Outbound calls to the Broker Back-Office are routed through
/// <paramref name="brokerHandler"/> (from <see cref="BrokerWebAppFactory"/>).
/// Uses an isolated temp content root.
/// </summary>
public sealed class ResearchAnalyticsFactory : WebApplicationFactory<FxWebPortal.Services.ArticleService>
{
    private readonly HttpMessageHandler _brokerHandler;

    public string TempRoot { get; } =
        Path.Combine(Path.GetTempPath(), $"fxtest_ra_{Guid.NewGuid():N}");

    public ResearchAnalyticsFactory(HttpMessageHandler brokerHandler)
        => _brokerHandler = brokerHandler;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        Directory.CreateDirectory(Path.Combine(TempRoot, "Data"));
        File.WriteAllText(Path.Combine(TempRoot, "Data", "articles.json"), "[]");
        File.WriteAllText(Path.Combine(TempRoot, "Data", "visitors.json"), "[]");

        builder.UseContentRoot(TempRoot);
        builder.UseEnvironment("Development");
        // Point the broker notification to a path the routing handler will intercept.
        builder.UseSetting("BrokerNotification:EndpointUrl", "http://localhost/api/accounts/leads");

        builder.ConfigureTestServices(services =>
        {
            // Route all outbound HTTP from the /api/track handler to the broker test server.
            services.AddSingleton<IHttpClientFactory>(
                new RoutingHttpClientFactory(_brokerHandler));
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing && Directory.Exists(TempRoot))
            // Best-effort cleanup of the temp content root created for test isolation.
            // A failure here does not invalidate test results.
            try { Directory.Delete(TempRoot, recursive: true); } catch { /* intentional: temp-dir cleanup is non-critical */ }
    }
}

// ── News Feed factory ────────────────────────────────────────────────────────

/// <summary>
/// WebApplicationFactory for the <c>news-feed</c> service.
/// Outbound calls via <c>NewsPublishService</c> are routed through
/// <paramref name="analyticsHandler"/> (from <see cref="ResearchAnalyticsFactory"/>).
/// </summary>
public sealed class NewsFeedFactory : WebApplicationFactory<FxWebNews.Services.NewsService>
{
    private readonly HttpMessageHandler _analyticsHandler;

    public NewsFeedFactory(HttpMessageHandler analyticsHandler)
        => _analyticsHandler = analyticsHandler;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        // Point news publish to a path the routing handler will intercept.
        builder.UseSetting("NewsPublish:EndpointUrl", "http://localhost/api/articles/receive");

        builder.ConfigureTestServices(services =>
        {
            // Route NewsPublishService's HTTP client to the research-analytics test server.
            services.AddSingleton<IHttpClientFactory>(
                new RoutingHttpClientFactory(_analyticsHandler));
        });
    }
}
