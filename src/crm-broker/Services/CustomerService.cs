using FxWebApi.Data;
using FxWebApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FxWebApi.Services
{
    public class CustomerService
    {
        private readonly FxDbContext _dbContext;
        private readonly ILogger<CustomerService> _logger;

        public CustomerService(FxDbContext dbContext, ILogger<CustomerService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<List<Customer>?> GetAllCustomersAsync()
        {
            try
            {
                return await _dbContext.Customers
                    .Include(c => c.Portfolios)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve customers from database");
                return null;
            }
        }

        public async Task<Customer?> GetCustomerAsync(int id)
        {
            try
            {
                return await _dbContext.Customers
                    .Include(c => c.Portfolios)
                    .FirstOrDefaultAsync(c => c.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve customer {Id}", id);
                return null;
            }
        }

        public async Task<List<CustomerHistory>?> GetCustomerHistoriesAsync(int customerId)
        {
            try
            {
                return await _dbContext.CustomerHistories
                    .Where(h => h.CustomerId == customerId)
                    .OrderByDescending(h => h.ClosedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve histories for customer {Id}", customerId);
                return null;
            }
        }

        public async Task<List<CustomerPortfolio>?> GetCustomerPortfoliosAsync(int customerId)
        {
            try
            {
                return await _dbContext.CustomerPortfolios
                    .Where(p => p.CustomerId == customerId)
                    .OrderByDescending(p => p.OpenedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve portfolios for customer {Id}", customerId);
                return null;
            }
        }

        public async Task<CustomerPreference?> GetCustomerPreferencesAsync(int customerId)
        {
            try
            {
                return await _dbContext.CustomerPreferences
                    .FirstOrDefaultAsync(p => p.CustomerId == customerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve preferences for customer {Id}", customerId);
                return null;
            }
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
                Histories = histories ?? new List<CustomerHistory>(),
                Portfolios = portfolios ?? new List<CustomerPortfolio>(),
                Preferences = preferences
            };
        }
    }

    public class CustomerProfileDto
    {
        public Customer Customer { get; set; } = null!;
        public List<CustomerHistory> Histories { get; set; } = new();
        public List<CustomerPortfolio> Portfolios { get; set; } = new();
        public CustomerPreference? Preferences { get; set; }
    }
}
