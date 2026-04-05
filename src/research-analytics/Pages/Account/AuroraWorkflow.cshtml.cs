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
        public string AuroraApiUrl { get; set; } = string.Empty;
        public string AuroraQuoteUrl { get; set; } = string.Empty;

        public AuroraWorkflowModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void OnGet(string customer, string pair, string signal)
        {
            CustomerName = customer ?? "Unknown Customer";
            CurrencyPair = pair ?? "AUD/USD";
            Signal = signal ?? "Buy";

            var brokerApiUrl = _configuration["BrokerCrmApiUrl"] ?? "http://localhost:5002";
            AuroraApiUrl = $"{brokerApiUrl}/api/aurora";
            AuroraQuoteUrl = $"{brokerApiUrl}/api/aurora/quote/audusd";
        }
    }
}
