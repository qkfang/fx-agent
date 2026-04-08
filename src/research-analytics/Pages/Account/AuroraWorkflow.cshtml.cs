using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FxWebPortal.Pages.Account
{
    public class AuroraWorkflowModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public string CustomerName { get; set; } = string.Empty;
        public string CurrencyPair { get; set; } = string.Empty;
        public string Signal { get; set; } = string.Empty;
        public string AuroraQuoteUrl { get; set; } = string.Empty;
        public string FoundryAgentUrl { get; set; } = string.Empty;
        public string CrmBrokerUrl { get; set; } = string.Empty;

        public AuroraWorkflowModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void OnGet(string customer, string pair, string signal)
        {
            CustomerName = customer ?? "Unknown Customer";
            CurrencyPair = pair ?? "AUD/USD";
            Signal = signal ?? "Buy";

            var tradingPlatformUrl = _configuration["TradingPlatformUrl"] ?? "http://localhost:5249";
            var pairCode = (pair ?? "AUD/USD").Replace("/", "").ToLowerInvariant();
            AuroraQuoteUrl = $"{tradingPlatformUrl}/api/quote/{pairCode}";

            FoundryAgentUrl = _configuration["FoundryAgent:EndpointUrl"]?.TrimEnd('/') ?? "http://localhost:5001";

            var crmBrokerEndpoint = _configuration["CrmBrokerApi:EndpointUrl"] ?? "http://localhost:5269/api/accounts/leads";
            var crmBaseUrl = crmBrokerEndpoint.Contains("/api/") 
                ? crmBrokerEndpoint.Substring(0, crmBrokerEndpoint.IndexOf("/api/"))
                : "http://localhost:5269";
            CrmBrokerUrl = crmBaseUrl;
        }
    }
}
