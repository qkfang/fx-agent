using Microsoft.AspNetCore.Mvc;
using FxWebApi.Services;

namespace FxWebApi.Controllers
{
    [ApiController]
    [Route("api/customers")]
    public class CustomerController : ControllerBase
    {
        private readonly CustomerService _customerService;
        private readonly ILogger<CustomerController> _logger;

        public CustomerController(CustomerService customerService, ILogger<CustomerController> logger)
        {
            _customerService = customerService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<CustomerDto>>> GetAll()
        {
            var customers = await _customerService.GetAllCustomersAsync();
            if (customers == null) return StatusCode(502, "Failed to retrieve customers from integration API");
            return Ok(customers);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CustomerDto>> Get(int id)
        {
            var customer = await _customerService.GetCustomerAsync(id);
            if (customer == null) return NotFound();
            return Ok(customer);
        }

        [HttpGet("{id}/histories")]
        public async Task<ActionResult<List<CustomerHistoryDto>>> GetHistories(int id)
        {
            var histories = await _customerService.GetCustomerHistoriesAsync(id);
            if (histories == null) return StatusCode(502, "Failed to retrieve customer histories");
            return Ok(histories);
        }

        [HttpGet("{id}/portfolios")]
        public async Task<ActionResult<List<CustomerPortfolioDto>>> GetPortfolios(int id)
        {
            var portfolios = await _customerService.GetCustomerPortfoliosAsync(id);
            if (portfolios == null) return StatusCode(502, "Failed to retrieve customer portfolios");
            return Ok(portfolios);
        }

        [HttpGet("{id}/preferences")]
        public async Task<ActionResult<CustomerPreferenceDto>> GetPreferences(int id)
        {
            var prefs = await _customerService.GetCustomerPreferencesAsync(id);
            if (prefs == null) return NotFound();
            return Ok(prefs);
        }

        [HttpGet("{id}/profile")]
        public async Task<ActionResult<CustomerProfileDto>> GetProfile(int id)
        {
            var profile = await _customerService.GetCustomerProfileAsync(id);
            if (profile == null) return NotFound();
            return Ok(profile);
        }
    }
}
