using System.Text.Json;

namespace FxWebApi.Services
{
    public class CustomerService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;
        private readonly ILogger<CustomerService> _logger;
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

        public CustomerService(IHttpClientFactory httpClientFactory, IConfiguration config, ILogger<CustomerService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;
            _logger = logger;
        }

        private string BaseUrl => _config["IntegrationApiUrl"] ?? "http://localhost:5005";

        public async Task<List<CustomerDto>?> GetAllCustomersAsync()
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"{BaseUrl}/api/Customers");
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to fetch customers: {Status}", response.StatusCode);
                return null;
            }
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<CustomerDto>>(json, JsonOptions);
        }

        public async Task<CustomerDto?> GetCustomerAsync(int id)
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"{BaseUrl}/api/Customers/{id}");
            if (!response.IsSuccessStatusCode) return null;
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<CustomerDto>(json, JsonOptions);
        }

        public async Task<List<CustomerHistoryDto>?> GetCustomerHistoriesAsync(int customerId)
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"{BaseUrl}/api/CustomerHistories/customer/{customerId}");
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to fetch histories for customer {Id}: {Status}", customerId, response.StatusCode);
                return null;
            }
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<CustomerHistoryDto>>(json, JsonOptions);
        }

        public async Task<List<CustomerPortfolioDto>?> GetCustomerPortfoliosAsync(int customerId)
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"{BaseUrl}/api/Portfolios/customer/{customerId}");
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to fetch portfolios for customer {Id}: {Status}", customerId, response.StatusCode);
                return null;
            }
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<CustomerPortfolioDto>>(json, JsonOptions);
        }

        public async Task<CustomerPreferenceDto?> GetCustomerPreferencesAsync(int customerId)
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"{BaseUrl}/api/CustomerPreferences/customer/{customerId}");
            if (!response.IsSuccessStatusCode) return null;
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<CustomerPreferenceDto>(json, JsonOptions);
        }

        public async Task<CustomerProfileDto?> GetCustomerProfileAsync(int customerId)
        {
            var customer = await GetCustomerAsync(customerId);
            if (customer == null) return null;

            var histories = await GetCustomerHistoriesAsync(customerId);
            var portfolios = await GetCustomerPortfoliosAsync(customerId);
            var preferences = await GetCustomerPreferencesAsync(customerId);

            return new CustomerProfileDto
            {
                Customer = customer,
                Histories = histories ?? new List<CustomerHistoryDto>(),
                Portfolios = portfolios ?? new List<CustomerPortfolioDto>(),
                Preferences = preferences
            };
        }
    }

    public class CustomerDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public List<CustomerPortfolioDto> Portfolios { get; set; } = new();
    }

    public class CustomerHistoryDto
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string CurrencyPair { get; set; } = string.Empty;
        public string Direction { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal EntryRate { get; set; }
        public decimal ExitRate { get; set; }
        public decimal PnL { get; set; }
        public DateTime OpenedAt { get; set; }
        public DateTime ClosedAt { get; set; }
        public string Notes { get; set; } = string.Empty;
    }

    public class CustomerPortfolioDto
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string CurrencyPair { get; set; } = string.Empty;
        public string Direction { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal EntryRate { get; set; }
        public DateTime OpenedAt { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class CustomerPreferenceDto
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string PreferredCurrencyPairs { get; set; } = string.Empty;
        public string RiskTolerance { get; set; } = string.Empty;
        public decimal MaxPositionSize { get; set; }
        public decimal StopLossPercent { get; set; }
        public decimal TakeProfitPercent { get; set; }
        public string TradingStyle { get; set; } = string.Empty;
        public bool EnableNotifications { get; set; }
        public string NotificationChannels { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; }
    }

    public class CustomerProfileDto
    {
        public CustomerDto Customer { get; set; } = null!;
        public List<CustomerHistoryDto> Histories { get; set; } = new();
        public List<CustomerPortfolioDto> Portfolios { get; set; } = new();
        public CustomerPreferenceDto? Preferences { get; set; }
    }
}
