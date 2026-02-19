using FxWebUI.Models;
using System.Text.Json;

namespace FxWebUI.Services
{
    public class FxDataService
    {
        private readonly IWebHostEnvironment _env;
        private readonly IHttpClientFactory _httpClientFactory;
        private List<Transaction> _transactions = new();
        private FundSummary _fundSummary = new();

        public FxDataService(IWebHostEnvironment env, IHttpClientFactory httpClientFactory)
        {
            _env = env;
            _httpClientFactory = httpClientFactory;
            LoadData();
        }

        private void LoadData()
        {
            var options = new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            };

            // Load transactions
            var transactionsPath = Path.Combine(_env.ContentRootPath, "Data", "transactions.json");
            if (File.Exists(transactionsPath))
            {
                var json = File.ReadAllText(transactionsPath);
                _transactions = JsonSerializer.Deserialize<List<Transaction>>(json, options) ?? new List<Transaction>();
            }

            // Load fund summary
            var fundPath = Path.Combine(_env.ContentRootPath, "Data", "fund.json");
            if (File.Exists(fundPath))
            {
                var json = File.ReadAllText(fundPath);
                _fundSummary = JsonSerializer.Deserialize<FundSummary>(json, options) ?? new FundSummary();
            }
        }

        public List<Transaction> GetTransactions()
        {
            return _transactions;
        }

        public FundSummary GetFundSummary()
        {
            return _fundSummary;
        }

        public async Task<FxRate?> GetCurrentFxRate()
        {
            try
            {
                // Call the FX API to get current rate
                var client = _httpClientFactory.CreateClient();
                // Default to localhost if FX API URL not configured
                var fxApiUrl = Environment.GetEnvironmentVariable("FX_API_URL") ?? "http://localhost:5001";
                var response = await client.GetAsync($"{fxApiUrl}/api/fx/rate");
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions 
                    { 
                        PropertyNameCaseInsensitive = true 
                    };
                    return JsonSerializer.Deserialize<FxRate>(json, options);
                }
            }
            catch
            {
                // Return mock data if API is not available
            }

            // Return mock data
            return new FxRate
            {
                CurrencyPair = "AUD/USD",
                Rate = 0.6550m,
                Timestamp = DateTime.UtcNow
            };
        }
    }
}
