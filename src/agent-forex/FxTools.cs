using System.ComponentModel;
using System.Net.Http.Json;

public class FxTools
{
    private readonly HttpClient _http;

    public FxTools(HttpClient httpClient)
    {
        _http = httpClient;
    }

    [Description("Get the current AUD/USD exchange rate quote with bid, ask, mid, and spread.")]
    public string GetFxQuote()
    {
        try
        {
            var response = _http.GetAsync("/api/fx/quote").Result;
            response.EnsureSuccessStatusCode();
            return response.Content.ReadAsStringAsync().Result;
        }
        catch (Exception ex)
        {
            return $"Error fetching quote: {ex.Message}";
        }
    }

    [Description("Get the current market status including trend direction, volatility, and day statistics for AUD/USD.")]
    public string GetMarketStatus()
    {
        try
        {
            var response = _http.GetAsync("/api/fx/status").Result;
            response.EnsureSuccessStatusCode();
            return response.Content.ReadAsStringAsync().Result;
        }
        catch (Exception ex)
        {
            return $"Error fetching market status: {ex.Message}";
        }
    }

    [Description("Get recent OHLC price history candles for AUD/USD.")]
    public string GetPriceHistory(
        [Description("Number of candle bars to retrieve (1-500, default 20)")] int bars = 20)
    {
        try
        {
            bars = Math.Clamp(bars, 1, 500);
            var response = _http.GetAsync($"/api/fx/history?bars={bars}").Result;
            response.EnsureSuccessStatusCode();
            return response.Content.ReadAsStringAsync().Result;
        }
        catch (Exception ex)
        {
            return $"Error fetching price history: {ex.Message}";
        }
    }

    [Description("Get all trading accounts with their summary information.")]
    public string GetAccounts()
    {
        try
        {
            var response = _http.GetAsync("/api/accounts").Result;
            response.EnsureSuccessStatusCode();
            return response.Content.ReadAsStringAsync().Result;
        }
        catch (Exception ex)
        {
            return $"Error fetching accounts: {ex.Message}";
        }
    }

    [Description("Get the balance sheet for a specific trading account, including open positions and recent transactions.")]
    public string GetAccountBalance(
        [Description("The account ID (e.g., 1, 2, or 3)")] int accountId)
    {
        try
        {
            var response = _http.GetAsync($"/api/accounts/{accountId}/balance").Result;
            response.EnsureSuccessStatusCode();
            return response.Content.ReadAsStringAsync().Result;
        }
        catch (Exception ex)
        {
            return $"Error fetching balance for account {accountId}: {ex.Message}";
        }
    }

    [Description("Execute a buy trade on AUD/USD for a specific account.")]
    public string ExecuteBuy(
        [Description("The account ID")] int accountId,
        [Description("Number of lots to buy (e.g., 0.1, 0.5, 1.0)")] decimal lots)
    {
        try
        {
            var content = JsonContent.Create(new { currencyPair = "AUD/USD", lots });
            var response = _http.PostAsync($"/api/accounts/{accountId}/buy", content).Result;
            return response.Content.ReadAsStringAsync().Result;
        }
        catch (Exception ex)
        {
            return $"Error executing buy: {ex.Message}";
        }
    }

    [Description("Execute a sell trade on AUD/USD for a specific account.")]
    public string ExecuteSell(
        [Description("The account ID")] int accountId,
        [Description("Number of lots to sell (e.g., 0.1, 0.5, 1.0)")] decimal lots)
    {
        try
        {
            var content = JsonContent.Create(new { currencyPair = "AUD/USD", lots });
            var response = _http.PostAsync($"/api/accounts/{accountId}/sell", content).Result;
            return response.Content.ReadAsStringAsync().Result;
        }
        catch (Exception ex)
        {
            return $"Error executing sell: {ex.Message}";
        }
    }

    [Description("Close an open position for an account.")]
    public string ClosePosition(
        [Description("The account ID")] int accountId,
        [Description("The position ID to close (e.g., POS001)")] string positionId)
    {
        try
        {
            var response = _http.PostAsync($"/api/accounts/{accountId}/close/{positionId}", null).Result;
            return response.Content.ReadAsStringAsync().Result;
        }
        catch (Exception ex)
        {
            return $"Error closing position: {ex.Message}";
        }
    }

    [Description("Get recent FX transaction records.")]
    public string GetTransactions(
        [Description("Maximum number of transactions to return (default 20)")] int limit = 20)
    {
        try
        {
            var response = _http.GetAsync($"/api/fx/transactions?limit={limit}").Result;
            response.EnsureSuccessStatusCode();
            return response.Content.ReadAsStringAsync().Result;
        }
        catch (Exception ex)
        {
            return $"Error fetching transactions: {ex.Message}";
        }
    }
}
