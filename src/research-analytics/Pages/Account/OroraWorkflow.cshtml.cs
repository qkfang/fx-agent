using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FxWebPortal.Pages.Account
{
    public class OroraWorkflowModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public string CustomerName { get; set; } = string.Empty;
        public string CurrencyPair { get; set; } = string.Empty;
        public string Signal { get; set; } = string.Empty;
        public string OroraApiUrl { get; set; } = string.Empty;
        public string OroraQuoteUrl { get; set; } = string.Empty;

        public OroraWorkflowModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void OnGet(string customer, string pair, string signal)
        {
            CustomerName = customer ?? "Unknown Customer";
            CurrencyPair = pair ?? "AUD/USD";
            Signal = signal ?? "Buy";

            var brokerApiUrl = _configuration["BrokerCrmApiUrl"] ?? "http://localhost:5002";
            OroraApiUrl = $"{brokerApiUrl}/api/orora";
            OroraQuoteUrl = $"{brokerApiUrl}/api/orora/quote/audusd";
        }
    }
}
