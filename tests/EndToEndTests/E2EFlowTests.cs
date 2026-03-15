using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

/// <summary>
/// End-to-end integration tests that exercise every API integration hop in the
/// FX Agent demo flow via real in-process HTTP calls:
///
///   News Feed  →  Research Analytics  →  Broker Back-Office  →  Trading Platform
///
/// Each service runs in its own <c>WebApplicationFactory</c> test server.
/// Cross-service HTTP calls are wired by injecting a <see cref="RoutingHttpClientFactory"/>
/// that routes requests to the correct downstream test server handler.
/// </summary>
public class E2EFlowTests : IAsyncLifetime
{
    // ── Factories ─────────────────────────────────────────────────────────────

    private TradingPlatformFactory _tradingPlatform = null!;
    private BrokerWebAppFactory _broker = null!;
    private ResearchAnalyticsFactory _analytics = null!;
    private NewsFeedFactory _newsFeed = null!;

    // ── Test clients ──────────────────────────────────────────────────────────

    private HttpClient _tradingClient = null!;
    private HttpClient _brokerClient = null!;
    private HttpClient _analyticsClient = null!;
    private HttpClient _newsFeedClient = null!;

    // ── Shared JSON options ───────────────────────────────────────────────────

    private static readonly JsonSerializerOptions _jsonOpts =
        new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

    /// <summary>
    /// Milliseconds to wait for fire-and-forget cross-service notifications
    /// (e.g. analytics→broker lead, broker→trading-platform settlement) to
    /// propagate through the in-process async task before asserting the result.
    /// </summary>
    private const int FireAndForgetPropagationMs = 600;

    // ── Setup / teardown ─────────────────────────────────────────────────────

    public Task InitializeAsync()
    {
        // Create factories in reverse dependency order so each factory already
        // has the downstream handler available when it is wired up.
        _tradingPlatform = new TradingPlatformFactory();
        _tradingClient = _tradingPlatform.CreateClient();

        _broker = new BrokerWebAppFactory(_tradingPlatform.Server.CreateHandler());
        _brokerClient = _broker.CreateClient();

        _analytics = new ResearchAnalyticsFactory(_broker.Server.CreateHandler());
        _analyticsClient = _analytics.CreateClient();

        _newsFeed = new NewsFeedFactory(_analytics.Server.CreateHandler());
        _newsFeedClient = _newsFeed.CreateClient();

        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        _newsFeedClient.Dispose();
        _newsFeed.Dispose();
        _analyticsClient.Dispose();
        _analytics.Dispose();
        _brokerClient.Dispose();
        _broker.Dispose();
        _tradingClient.Dispose();
        _tradingPlatform.Dispose();
        return Task.CompletedTask;
    }

    // ── Helper ────────────────────────────────────────────────────────────────

    private static StringContent Json(object payload) =>
        new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

    // =========================================================================
    // Broker Back-Office API tests
    // =========================================================================

    /// <summary>GET /api/fx/rate returns a valid AUD/USD rate from the live simulation.</summary>
    [Fact]
    public async Task BrokerApi_GetFxRate_ReturnsAudUsdRate()
    {
        var response = await _brokerClient.GetAsync("/api/fx/rate");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = doc.RootElement;

        Assert.Equal("AUD/USD", root.GetProperty("currencyPair").GetString());
        var rate = root.GetProperty("rate").GetDecimal();
        Assert.InRange(rate, 0.5m, 0.9m); // realistic AUD/USD range
    }

    /// <summary>GET /api/fx/quote returns bid, ask, spread for AUD/USD.</summary>
    [Fact]
    public async Task BrokerApi_GetFxQuote_ReturnsBidAskSpread()
    {
        var response = await _brokerClient.GetAsync("/api/fx/quote");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = doc.RootElement;

        var bid = root.GetProperty("bid").GetDecimal();
        var ask = root.GetProperty("ask").GetDecimal();
        Assert.True(ask > bid, "Ask must be greater than bid");
        Assert.InRange(bid, 0.5m, 0.9m);
        Assert.InRange(ask, 0.5m, 0.9m);
    }

    /// <summary>GET /api/accounts returns the three pre-seeded customer accounts.</summary>
    [Fact]
    public async Task BrokerApi_GetAccounts_ReturnsThreeAccounts()
    {
        var response = await _brokerClient.GetAsync("/api/accounts");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var accounts = JsonSerializer.Deserialize<JsonElement[]>(
            await response.Content.ReadAsStringAsync(), _jsonOpts)!;

        Assert.Equal(3, accounts.Length);
        // Verify account 1 is James Wilson
        var james = accounts.First(a => a.GetProperty("id").GetInt32() == 1);
        Assert.Equal("James Wilson", james.GetProperty("customerName").GetString());
    }

    /// <summary>GET /api/accounts/1/balance returns a populated balance sheet.</summary>
    [Fact]
    public async Task BrokerApi_GetBalanceSheet_ReturnsEquityAndPositions()
    {
        var response = await _brokerClient.GetAsync("/api/accounts/1/balance");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = doc.RootElement;

        Assert.Equal(1, root.GetProperty("accountId").GetInt32());
        Assert.True(root.GetProperty("balance").GetDecimal() > 0);
        Assert.True(root.GetProperty("equity").GetDecimal() > 0);
    }

    // =========================================================================
    // Broker MCP endpoint tests
    // =========================================================================

    /// <summary>POST /mcp/fx with action=quote returns the current AUD/USD quote.</summary>
    [Fact]
    public async Task BrokerMcp_QuoteAction_ReturnsBidAskQuote()
    {
        var response = await _brokerClient.PostAsync("/mcp/fx",
            Json(new { action = "quote", amount = 0 }));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = doc.RootElement;

        Assert.True(root.GetProperty("success").GetBoolean());
        var data = root.GetProperty("data");
        Assert.Equal("AUD/USD", data.GetProperty("currencyPair").GetString());
        Assert.True(data.GetProperty("bid").GetDecimal() > 0);
    }

    /// <summary>POST /mcp/fx with action=buy executes a trade and returns a transaction record.</summary>
    [Fact]
    public async Task BrokerMcp_BuyAction_ExecutesTradeSuccessfully()
    {
        var response = await _brokerClient.PostAsync("/mcp/fx",
            Json(new { action = "buy", amount = 10000 }));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = doc.RootElement;

        Assert.True(root.GetProperty("success").GetBoolean());
        var data = root.GetProperty("data");
        Assert.Equal("Buy", data.GetProperty("type").GetString());
        Assert.Equal(10000m, data.GetProperty("amount").GetDecimal());
    }

    /// <summary>POST /mcp/call with tool=fx_buy executes a trade via the structured MCP call endpoint.</summary>
    [Fact]
    public async Task BrokerMcp_StructuredCall_FxBuy_ExecutesTrade()
    {
        var response = await _brokerClient.PostAsync("/mcp/call",
            Json(new { tool = "fx_buy", parameters = new { amount = 5000 } }));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.True(doc.RootElement.GetProperty("success").GetBoolean());
    }

    // =========================================================================
    // Trading Platform API tests
    // =========================================================================

    /// <summary>GET /api/portfolio returns the initial fund summary.</summary>
    [Fact]
    public async Task TradingPlatform_GetPortfolio_ReturnsInitialFundSummary()
    {
        var response = await _tradingClient.GetAsync("/api/portfolio");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = doc.RootElement;

        Assert.Equal(100000m, root.GetProperty("totalBalance").GetDecimal());
        Assert.Equal(50000m, root.GetProperty("audBalance").GetDecimal());
        Assert.Equal(35000m, root.GetProperty("usdBalance").GetDecimal());
    }

    /// <summary>GET /api/trades returns the initial (empty) trade list from the temp data root.</summary>
    [Fact]
    public async Task TradingPlatform_GetTrades_ReturnsEmptyListInitially()
    {
        var response = await _tradingClient.GetAsync("/api/trades");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var trades = JsonSerializer.Deserialize<JsonElement[]>(
            await response.Content.ReadAsStringAsync(), _jsonOpts)!;
        Assert.Empty(trades);
    }

    /// <summary>POST /api/trades records the trade and updates AUD and USD balances.</summary>
    [Fact]
    public async Task TradingPlatform_PostTrade_RecordsTradeAndUpdatesFundBalance()
    {
        // Settle a buy trade directly on the trading platform
        var response = await _tradingClient.PostAsync("/api/trades", Json(new
        {
            type = "Buy",
            currencyPair = "AUD/USD",
            amount = 20000m,
            rate = 0.6550m,
            total = 13100m,
            dateTime = DateTime.UtcNow
        }));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
        Assert.True(result.GetProperty("settled").GetBoolean());
        Assert.True(result.GetProperty("id").GetInt32() >= 1);

        // Verify the trade appears in the list
        var tradesResp = await _tradingClient.GetAsync("/api/trades");
        var trades = JsonSerializer.Deserialize<JsonElement[]>(
            await tradesResp.Content.ReadAsStringAsync(), _jsonOpts)!;
        Assert.Single(trades);
        Assert.Equal("Buy", trades[0].GetProperty("type").GetString());
        Assert.Equal(20000m, trades[0].GetProperty("amount").GetDecimal());

        // Verify fund balances updated: AUD + 20000, USD - 13100
        var portfolioResp = await _tradingClient.GetAsync("/api/portfolio");
        var portfolio = JsonDocument.Parse(
            await portfolioResp.Content.ReadAsStringAsync()).RootElement;
        Assert.Equal(70000m, portfolio.GetProperty("audBalance").GetDecimal());
        Assert.Equal(21900m, portfolio.GetProperty("usdBalance").GetDecimal());
    }

    // =========================================================================
    // Research Analytics API tests
    // =========================================================================

    /// <summary>
    /// POST /api/articles/receive with a "Bad" FX news article creates a
    /// Published, Bearish research note in Research Analytics.
    /// </summary>
    [Fact]
    public async Task ResearchAnalytics_ReceiveNewsArticle_CreatesPublishedBearishNote()
    {
        var response = await _analyticsClient.PostAsync("/api/articles/receive", Json(new
        {
            id = 99,
            title = "Middle East Conflict Escalates – Risk-Off Sentiment Spreads to FX Markets",
            summary = "AUD/USD weakens as safe-haven demand rises amid Middle East tensions.",
            content = "Heightened military activity in the Middle East sent shockwaves through FX markets.",
            type = "Bad",
            category = "FX",
            author = "FX News Team",
            publishedAt = DateTime.UtcNow
        }));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
        Assert.True(result.GetProperty("received").GetBoolean());
        var articleId = result.GetProperty("articleId").GetInt32();
        Assert.True(articleId >= 1);

        // Verify the article was persisted in the ArticleService singleton
        var articleService = _analytics.Services
            .GetRequiredService<FxWebPortal.Services.ArticleService>();
        var created = articleService.GetById(articleId);

        Assert.NotNull(created);
        Assert.Equal("Bearish", created!.Sentiment);
        Assert.Equal("Published", created.Status);
        Assert.Equal("FX", created.Category);
        Assert.Contains("Middle East", created.Title);
        Assert.Contains("NewsAlert", created.Tags);
    }

    /// <summary>
    /// POST /api/articles/receive with a "Good" news article produces a Bullish research note.
    /// </summary>
    [Fact]
    public async Task ResearchAnalytics_ReceiveGoodNewsArticle_CreatesBullishNote()
    {
        var response = await _analyticsClient.PostAsync("/api/articles/receive", Json(new
        {
            id = 100,
            title = "China Stimulus Boosts AUD/USD Outlook",
            summary = "AUD rallies on broad Chinese stimulus package.",
            content = "Beijing announced a CNY 2 trillion fiscal stimulus, lifting commodity prices.",
            type = "Good",
            category = "AUD/USD",
            author = "Research Team",
            publishedAt = DateTime.UtcNow
        }));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
        Assert.True(result.GetProperty("received").GetBoolean());
        var articleId = result.GetProperty("articleId").GetInt32();

        var articleService = _analytics.Services
            .GetRequiredService<FxWebPortal.Services.ArticleService>();
        var created = articleService.GetById(articleId);

        Assert.NotNull(created);
        Assert.Equal("Bullish", created!.Sentiment);
        Assert.Equal("Published", created.Status);
    }

    // =========================================================================
    // Cross-service: Research Analytics → Broker Back-Office lead notification
    // =========================================================================

    /// <summary>
    /// POST /api/track on Research Analytics with a customer email triggers a
    /// fire-and-forget POST /api/accounts/leads to Broker Back-Office.
    /// Broker receives and stores the lead (GET /api/accounts/leads).
    /// </summary>
    [Fact]
    public async Task ResearchAnalytics_CustomerTracksArticleWithEmail_BrokerReceivesLead()
    {
        // Step 1 – create an article so we have a valid articleId
        var createResp = await _analyticsClient.PostAsync("/api/articles/receive", Json(new
        {
            id = 50,
            title = "AUD/USD Lead Test Article",
            summary = "Test article for lead tracking.",
            content = "Content of test article.",
            type = "Bad",
            category = "FX",
            author = "Test Author",
            publishedAt = DateTime.UtcNow
        }));
        Assert.Equal(HttpStatusCode.OK, createResp.StatusCode);
        var articleId = JsonDocument.Parse(
            await createResp.Content.ReadAsStringAsync()).RootElement
            .GetProperty("articleId").GetInt32();

        // Step 2 – customer reads article and submits their contact details
        var trackResp = await _analyticsClient.PostAsync("/api/track", Json(new
        {
            sessionId = "test-session-lead-001",
            articleId,
            pageUrl = $"/Article?id={articleId}",
            timeSpentSeconds = 120,
            clickCount = 5,
            userName = "James Wilson",
            userEmail = "james.wilson@email.com",
            userCompany = "Wilson Capital",
            language = "en-AU",
            timezone = "Australia/Sydney",
            screenSize = "1920x1080",
            referrer = ""
        }));
        Assert.Equal(HttpStatusCode.OK, trackResp.StatusCode);

        // The broker notification is fire-and-forget; allow time for propagation.
        await Task.Delay(FireAndForgetPropagationMs);

        // Step 3 – verify broker received the lead
        var leadsResp = await _brokerClient.GetAsync("/api/accounts/leads");
        Assert.Equal(HttpStatusCode.OK, leadsResp.StatusCode);

        var leads = JsonSerializer.Deserialize<JsonElement[]>(
            await leadsResp.Content.ReadAsStringAsync(), _jsonOpts)!;

        Assert.NotEmpty(leads);
        var lead = leads.First(l =>
            l.GetProperty("userEmail").GetString() == "james.wilson@email.com");

        Assert.Equal("James Wilson", lead.GetProperty("userName").GetString());
        Assert.Equal("Wilson Capital", lead.GetProperty("userCompany").GetString());
        Assert.Equal(articleId, lead.GetProperty("articleId").GetInt32());
        Assert.Equal(120, lead.GetProperty("timeSpentSeconds").GetInt32());
    }

    /// <summary>
    /// POST /api/track without an email should NOT create a broker lead.
    /// </summary>
    [Fact]
    public async Task ResearchAnalytics_AnonymousVisitor_DoesNotCreateBrokerLead()
    {
        var leadsBeforeResp = await _brokerClient.GetAsync("/api/accounts/leads");
        var countBefore = JsonSerializer.Deserialize<JsonElement[]>(
            await leadsBeforeResp.Content.ReadAsStringAsync(), _jsonOpts)!.Length;

        await _analyticsClient.PostAsync("/api/track", Json(new
        {
            sessionId = "anon-session-002",
            articleId = (int?)null,
            pageUrl = "/",
            timeSpentSeconds = 30,
            clickCount = 1,
            userName = "",
            userEmail = "",   // no email → broker must NOT be notified
            userCompany = "",
            language = "en",
            timezone = "UTC",
            screenSize = "1440x900",
            referrer = ""
        }));

        await Task.Delay(FireAndForgetPropagationMs / 2);

        var leadsAfterResp = await _brokerClient.GetAsync("/api/accounts/leads");
        var countAfter = JsonSerializer.Deserialize<JsonElement[]>(
            await leadsAfterResp.Content.ReadAsStringAsync(), _jsonOpts)!.Length;

        Assert.Equal(countBefore, countAfter);
    }

    // =========================================================================
    // Cross-service: Broker Back-Office → Trading Platform settlement
    // =========================================================================

    /// <summary>
    /// POST /api/accounts/1/buy on Broker fires a fire-and-forget POST /api/trades
    /// to Trading Platform.  The trade must appear in GET /api/trades and the
    /// AUD fund balance must increase.
    /// </summary>
    [Fact]
    public async Task BrokerBackOffice_ExecuteBuy_SettlesTradeOnTradingPlatform()
    {
        var portfolioBefore = JsonDocument.Parse(
            await (await _tradingClient.GetAsync("/api/portfolio"))
                .Content.ReadAsStringAsync()).RootElement;
        var audBefore = portfolioBefore.GetProperty("audBalance").GetDecimal();
        var usdBefore = portfolioBefore.GetProperty("usdBalance").GetDecimal();

        // Broker executes a Buy order: 1 lot = 100,000 AUD notional
        var buyResp = await _brokerClient.PostAsync("/api/accounts/1/buy",
            Json(new { lots = 1.0 }));
        Assert.Equal(HttpStatusCode.OK, buyResp.StatusCode);

        var result = JsonDocument.Parse(await buyResp.Content.ReadAsStringAsync()).RootElement;
        Assert.True(result.GetProperty("success").GetBoolean());
        Assert.Contains("Buy", result.GetProperty("message").GetString()!);

        // Fire-and-forget settlement propagates asynchronously; wait briefly.
        await Task.Delay(FireAndForgetPropagationMs);

        // Verify the trade appeared on the trading platform
        var tradesResp = await _tradingClient.GetAsync("/api/trades");
        Assert.Equal(HttpStatusCode.OK, tradesResp.StatusCode);
        var trades = JsonSerializer.Deserialize<JsonElement[]>(
            await tradesResp.Content.ReadAsStringAsync(), _jsonOpts)!;

        Assert.NotEmpty(trades);
        var settlement = trades.First(t => t.GetProperty("type").GetString() == "Buy");
        Assert.Equal("AUD/USD", settlement.GetProperty("currencyPair").GetString());
        Assert.Equal(100000m, settlement.GetProperty("amount").GetDecimal());

        // Verify AUD balance increased (bought AUD) and USD balance decreased
        var portfolioAfter = JsonDocument.Parse(
            await (await _tradingClient.GetAsync("/api/portfolio"))
                .Content.ReadAsStringAsync()).RootElement;

        Assert.True(portfolioAfter.GetProperty("audBalance").GetDecimal() > audBefore,
            "AUD balance should have increased after buying AUD");
        Assert.True(portfolioAfter.GetProperty("usdBalance").GetDecimal() < usdBefore,
            "USD balance should have decreased after buying AUD");
    }

    /// <summary>
    /// POST /api/accounts/2/sell on Broker fires settlement; trading platform
    /// records a Sell trade and AUD balance decreases.
    /// </summary>
    [Fact]
    public async Task BrokerBackOffice_ExecuteSell_SettlesSellTradeOnTradingPlatform()
    {
        var sellResp = await _brokerClient.PostAsync("/api/accounts/2/sell",
            Json(new { lots = 0.5 }));
        Assert.Equal(HttpStatusCode.OK, sellResp.StatusCode);

        var result = JsonDocument.Parse(await sellResp.Content.ReadAsStringAsync()).RootElement;
        Assert.True(result.GetProperty("success").GetBoolean());

        await Task.Delay(FireAndForgetPropagationMs);

        var tradesResp = await _tradingClient.GetAsync("/api/trades");
        var trades = JsonSerializer.Deserialize<JsonElement[]>(
            await tradesResp.Content.ReadAsStringAsync(), _jsonOpts)!;

        Assert.NotEmpty(trades);
        Assert.Contains(trades, t => t.GetProperty("type").GetString() == "Sell");
    }

    // =========================================================================
    // Cross-service: News Feed → Research Analytics via NewsPublishService
    // =========================================================================

    /// <summary>
    /// NewsPublishService.PushArticleAsync (news-feed) sends a POST to
    /// /api/articles/receive on Research Analytics and verifies the article
    /// is created.
    /// </summary>
    [Fact]
    public async Task NewsFeed_PublishArticle_CreatesResearchNoteViaHttpCall()
    {
        // Access NewsPublishService directly from the test server's service container
        using var scope = _newsFeed.Services.CreateScope();
        var publishService = scope.ServiceProvider
            .GetRequiredService<FxWebNews.Services.NewsPublishService>();

        var article = new FxWebNews.Models.NewsArticle
        {
            Id = 77,
            Title = "War Escalates in Middle East – Safe-Haven Demand Surges",
            Summary = "AUD/USD under pressure as conflict spreads across the region.",
            Content = "Full-scale hostilities have broken out, triggering broad risk-off moves in FX.",
            Type = "Bad",
            Category = "FX",
            Author = "FX News Team",
            IsPublished = true,
            PublishedAt = DateTime.UtcNow
        };

        var pushResult = await publishService.PushArticleAsync(article);

        Assert.True(pushResult.Success, $"NewsPublishService.PushArticleAsync failed: {pushResult.Message}");
        Assert.Contains("successfully", pushResult.Message, StringComparison.OrdinalIgnoreCase);

        // Verify research analytics now has the article
        var articleService = _analytics.Services
            .GetRequiredService<FxWebPortal.Services.ArticleService>();
        var published = articleService.GetPublished();

        Assert.Contains(published, a =>
            a.Title.Contains("War Escalates") && a.Sentiment == "Bearish");
    }

    // =========================================================================
    // Full end-to-end flow: news article → lead capture → trade → settlement
    // =========================================================================

    /// <summary>
    /// Full scenario:
    ///   1. News feed publishes a war-news article to Research Analytics.
    ///   2. Research Analytics auto-creates a Bearish research note.
    ///   3. Customer reads the article and submits their contact details.
    ///   4. Broker Back-Office receives the customer lead notification.
    ///   5. Broker executes a Buy AUD/USD order for the customer.
    ///   6. Trading Platform records the settled trade.
    ///   7. Portfolio AUD balance is updated.
    /// </summary>
    [Fact]
    public async Task FullFlow_WarNews_TriggersResearchNote_LeadCapture_BrokerTrade_Settlement()
    {
        // ── Step 1: News Feed publishes war-news article ──────────────────────
        using var scope = _newsFeed.Services.CreateScope();
        var publishService = scope.ServiceProvider
            .GetRequiredService<FxWebNews.Services.NewsPublishService>();

        var newsArticle = new FxWebNews.Models.NewsArticle
        {
            Id = 200,
            Title = "War in Middle East Drives AUD/USD to 4-Month Low",
            Summary = "Safe-haven demand sends AUD tumbling as Middle East conflict widens.",
            Content = "Escalating Middle East hostilities have rattled global FX markets. " +
                      "The Australian dollar fell sharply as investors fled to the US dollar.",
            Type = "Bad",
            Category = "FX",
            Author = "FX News Team",
            IsPublished = true,
            PublishedAt = DateTime.UtcNow
        };

        var publishResult = await publishService.PushArticleAsync(newsArticle);
        Assert.True(publishResult.Success, $"Step 1 (news feed publish) failed: {publishResult.Message}");

        // ── Step 2: Verify Research Analytics created a Bearish research note ─
        var articleService = _analytics.Services
            .GetRequiredService<FxWebPortal.Services.ArticleService>();
        var publishedArticles = articleService.GetPublished();
        var researchNote = publishedArticles
            .FirstOrDefault(a => a.Title.Contains("Middle East"));
        Assert.NotNull(researchNote);
        Assert.Equal("Bearish", researchNote!.Sentiment);
        Assert.Equal("Published", researchNote.Status);

        // ── Step 3: Customer reads article and submits contact details ─────────
        var trackResp = await _analyticsClient.PostAsync("/api/track", Json(new
        {
            sessionId = "e2e-session-fullflow",
            articleId = researchNote.Id,
            pageUrl = $"/Article?id={researchNote.Id}",
            timeSpentSeconds = 180,
            clickCount = 8,
            userName = "James Wilson",
            userEmail = "james.wilson@email.com",
            userCompany = "Wilson Capital",
            language = "en-AU",
            timezone = "Australia/Sydney",
            screenSize = "1920x1080",
            referrer = "https://google.com"
        }));
        Assert.Equal(HttpStatusCode.OK, trackResp.StatusCode);

        // Allow broker lead notification to propagate
        await Task.Delay(FireAndForgetPropagationMs);

        // ── Step 4: Broker Back-Office has received the customer lead ──────────
        var leadsResp = await _brokerClient.GetAsync("/api/accounts/leads");
        Assert.Equal(HttpStatusCode.OK, leadsResp.StatusCode);
        var leads = JsonSerializer.Deserialize<JsonElement[]>(
            await leadsResp.Content.ReadAsStringAsync(), _jsonOpts)!;

        var customerLead = leads.FirstOrDefault(l =>
            l.GetProperty("userEmail").GetString() == "james.wilson@email.com");
        Assert.True(customerLead.ValueKind != JsonValueKind.Undefined,
            "Broker should have a lead for james.wilson@email.com");
        Assert.Equal(researchNote.Id, customerLead.GetProperty("articleId").GetInt32());

        // ── Step 5: Broker executes Buy AUD/USD for account 1 (James Wilson) ──
        var tradeBefore = JsonDocument.Parse(
            await (await _tradingClient.GetAsync("/api/portfolio"))
                .Content.ReadAsStringAsync()).RootElement;
        var audBefore = tradeBefore.GetProperty("audBalance").GetDecimal();

        var buyResp = await _brokerClient.PostAsync("/api/accounts/1/buy",
            Json(new { lots = 1.0 }));
        Assert.Equal(HttpStatusCode.OK, buyResp.StatusCode);
        var buyResult = JsonDocument.Parse(
            await buyResp.Content.ReadAsStringAsync()).RootElement;
        Assert.True(buyResult.GetProperty("success").GetBoolean(),
            "Broker buy order should succeed");
        var rate = buyResult.GetProperty("transaction")
                            .GetProperty("rate").GetDecimal();
        Assert.InRange(rate, 0.5m, 0.9m);

        // Allow fire-and-forget trade settlement to propagate
        await Task.Delay(FireAndForgetPropagationMs);

        // ── Step 6: Trading Platform has the settled trade ────────────────────
        var tradesResp = await _tradingClient.GetAsync("/api/trades");
        Assert.Equal(HttpStatusCode.OK, tradesResp.StatusCode);
        var trades = JsonSerializer.Deserialize<JsonElement[]>(
            await tradesResp.Content.ReadAsStringAsync(), _jsonOpts)!;

        Assert.NotEmpty(trades);
        var settled = trades.First(t => t.GetProperty("type").GetString() == "Buy");
        Assert.Equal("AUD/USD", settled.GetProperty("currencyPair").GetString());
        Assert.Equal(100000m, settled.GetProperty("amount").GetDecimal());
        Assert.True(settled.GetProperty("rate").GetDecimal() > 0);

        // ── Step 7: Portfolio balance updated ─────────────────────────────────
        var portfolioAfter = JsonDocument.Parse(
            await (await _tradingClient.GetAsync("/api/portfolio"))
                .Content.ReadAsStringAsync()).RootElement;
        var audAfter = portfolioAfter.GetProperty("audBalance").GetDecimal();

        Assert.True(audAfter > audBefore,
            $"AUD balance should have increased. Before={audBefore}, After={audAfter}");
    }
}
