using System.ComponentModel;
using System.Net.Http.Json;

namespace FxWebUI.Tools
{
    public class TradingPlatformTools
    {
        private readonly HttpClient _http;

        public TradingPlatformTools(HttpClient httpClient)
        {
            _http = httpClient;
        }

        [Description("Get trading transaction history with details of all buy and sell activities.")]
        public string GetTransactions(
            [Description("Maximum number of transactions to return (default 50)")] int limit = 50)
        {
            try
            {
                var response = _http.PostAsJsonAsync("/mcp/call", new
                {
                    tool = "get_transactions",
                    parameters = new { limit }
                }).Result;

                response.EnsureSuccessStatusCode();
                return response.Content.ReadAsStringAsync().Result;
            }
            catch (Exception ex)
            {
                return $"Error fetching transactions: {ex.Message}";
            }
        }

        [Description("Get fund portfolio summary including total balance, AUD balance, USD balance, and profit/loss.")]
        public string GetFundSummary()
        {
            try
            {
                var response = _http.PostAsJsonAsync("/mcp/call", new
                {
                    tool = "get_fund_summary",
                    parameters = new { }
                }).Result;

                response.EnsureSuccessStatusCode();
                return response.Content.ReadAsStringAsync().Result;
            }
            catch (Exception ex)
            {
                return $"Error fetching fund summary: {ex.Message}";
            }
        }

        [Description("Get the most recent transaction.")]
        public string GetLatestTransaction()
        {
            try
            {
                var response = _http.PostAsJsonAsync("/mcp/call", new
                {
                    tool = "get_latest_transaction",
                    parameters = new { }
                }).Result;

                response.EnsureSuccessStatusCode();
                return response.Content.ReadAsStringAsync().Result;
            }
            catch (Exception ex)
            {
                return $"Error fetching latest transaction: {ex.Message}";
            }
        }

        [Description("Get a specific transaction by ID.")]
        public string GetTransactionById(
            [Description("Transaction ID")] int id)
        {
            try
            {
                var response = _http.PostAsJsonAsync("/mcp/call", new
                {
                    tool = "get_transaction_by_id",
                    parameters = new { id }
                }).Result;

                response.EnsureSuccessStatusCode();
                return response.Content.ReadAsStringAsync().Result;
            }
            catch (Exception ex)
            {
                return $"Error fetching transaction {id}: {ex.Message}";
            }
        }
    }
}
