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
        private readonly object _lock = new();

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

        private void SaveTransactions()
        {
            var transactionsPath = Path.Combine(_env.ContentRootPath, "Data", "transactions.json");
            var json = JsonSerializer.Serialize(_transactions, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(transactionsPath, json);
        }

        public List<Transaction> GetTransactions()
        {
            lock (_lock) return _transactions.OrderByDescending(t => t.DateTime).ToList();
        }

        public FundSummary GetFundSummary()
        {
            lock (_lock) return _fundSummary;
        }

        /// <summary>Record an incoming settled trade from the Broker Back-Office.</summary>
        public Transaction AddTransaction(Transaction transaction)
        {
            lock (_lock)
            {
                transaction.Id = _transactions.Any() ? _transactions.Max(t => t.Id) + 1 : 1;
                if (transaction.DateTime == default) transaction.DateTime = DateTime.UtcNow;
                _transactions.Add(transaction);

                // Update fund summary balances
                if (transaction.Type == "Buy")
                {
                    _fundSummary.AudBalance += transaction.Amount;
                    _fundSummary.UsdBalance -= transaction.Total;
                }
                else if (transaction.Type == "Sell")
                {
                    _fundSummary.AudBalance -= transaction.Amount;
                    _fundSummary.UsdBalance += transaction.Total;
                }
                _fundSummary.TotalBalance = _fundSummary.AudBalance + _fundSummary.UsdBalance;

                SaveTransactions();
                return transaction;
            }
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
