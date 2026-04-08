using FxWebApi.Data;
using FxWebApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;

namespace FxWebApi.Services
{
    public class AccountService
    {
        private readonly FxRateService _fxRateService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;
        private readonly ILogger<AccountService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly List<LeadNotification> _leads = new();
        private readonly List<TradeNotification> _tradeNotifications = new();
        private readonly object _lock = new();

        public AccountService(FxRateService fxRateService, IHttpClientFactory httpClientFactory,
            IConfiguration config, ILogger<AccountService> logger, IServiceScopeFactory scopeFactory)
        {
            _fxRateService = fxRateService;
            _httpClientFactory = httpClientFactory;
            _config = config;
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        private decimal CalculatePnL(Position pos, decimal currentRate)
        {
            var units = pos.Lots * 100000m;
            return pos.Type == "Buy"
                ? (currentRate - pos.OpenRate) * units
                : (pos.OpenRate - currentRate) * units;
        }

        private decimal CalculateMargin(decimal lots, decimal rate, decimal leverage)
        {
            return Math.Round((lots * 100000m * rate) / leverage, 2);
        }

        private Account MapCustomerToAccount(Customer customer, decimal balance)
        {
            return new Account
            {
                Id = customer.Id,
                AccountNumber = $"FX{10000 + customer.Id}",
                CustomerName = customer.Name,
                Email = customer.Email,
                Country = customer.Company,
                AccountType = "Standard",
                Status = "Active",
                Balance = balance,
                Leverage = 100m,
                CreatedAt = customer.CreatedAt
            };
        }

        private List<Position> MapPortfoliosToPositions(IEnumerable<CustomerPortfolio> portfolios, decimal leverage)
        {
            return portfolios
                .Where(p => p.Status == "Open")
                .Select(p => new Position
                {
                    PositionId = $"POS{p.Id}",
                    AccountId = p.CustomerId,
                    CurrencyPair = p.CurrencyPair,
                    Type = p.Direction,
                    Lots = p.Amount / 100000m,
                    OpenRate = p.EntryRate,
                    CurrentRate = p.EntryRate,
                    PnL = 0m,
                    Margin = Math.Round((p.Amount / 100000m * p.EntryRate * 100000m) / leverage, 2),
                    OpenTime = p.OpenedAt
                })
                .ToList();
        }

        private (List<AccountTransaction> transactions, decimal balance) BuildTransactions(int customerId, DateTime createdAt, List<CustomerHistory> histories)
        {
            var txList = new List<AccountTransaction>
            {
                new AccountTransaction
                {
                    TransactionId = $"T{customerId}00",
                    AccountId = customerId,
                    Type = "Deposit",
                    CurrencyPair = "-",
                    Lots = 0,
                    Rate = 0,
                    PnL = 0,
                    BalanceAfter = 50000m,
                    Timestamp = createdAt
                }
            };

            decimal balance = 50000m;
            foreach (var h in histories)
            {
                txList.Add(new AccountTransaction
                {
                    TransactionId = $"T{h.Id}",
                    AccountId = customerId,
                    Type = h.Direction,
                    CurrencyPair = h.CurrencyPair,
                    Lots = h.Amount / 100000m,
                    Rate = h.EntryRate,
                    PnL = 0,
                    BalanceAfter = balance,
                    Timestamp = h.OpenedAt
                });

                balance += h.PnL;
                txList.Add(new AccountTransaction
                {
                    TransactionId = $"TC{h.Id}",
                    AccountId = customerId,
                    Type = $"Close {h.Direction}",
                    CurrencyPair = h.CurrencyPair,
                    Lots = h.Amount / 100000m,
                    Rate = h.ExitRate,
                    PnL = h.PnL,
                    BalanceAfter = balance,
                    Timestamp = h.ClosedAt
                });
            }

            return (txList, balance);
        }

        public List<AccountSummary> GetAllAccounts()
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<FxDbContext>();

            var customers = db.Customers.Include(c => c.Portfolios).ToList();
            var currentRate = _fxRateService.GetCurrentRate().Rate;
            var summaries = new List<AccountSummary>();

            foreach (var customer in customers)
            {
                var histories = db.CustomerHistories
                    .Where(h => h.CustomerId == customer.Id)
                    .OrderBy(h => h.OpenedAt)
                    .ToList();

                var (_, balance) = BuildTransactions(customer.Id, customer.CreatedAt, histories);
                var positions = MapPortfoliosToPositions(customer.Portfolios, 100m);
                var openPnL = positions.Sum(p => CalculatePnL(p, currentRate));

                summaries.Add(new AccountSummary
                {
                    Id = customer.Id,
                    AccountNumber = $"FX{10000 + customer.Id}",
                    CustomerName = customer.Name,
                    AccountType = "Standard",
                    Status = "Active",
                    Country = customer.Company,
                    Balance = balance,
                    Equity = Math.Round(balance + openPnL, 2),
                    OpenPnL = Math.Round(openPnL, 2),
                    OpenPositionsCount = positions.Count,
                    Leverage = 100m,
                    CreatedAt = customer.CreatedAt
                });
            }

            return summaries;
        }

        public BalanceSheet? GetBalanceSheet(int accountId)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<FxDbContext>();

            var customer = db.Customers.Include(c => c.Portfolios).FirstOrDefault(c => c.Id == accountId);
            if (customer == null) return null;

            var histories = db.CustomerHistories
                .Where(h => h.CustomerId == accountId)
                .OrderBy(h => h.OpenedAt)
                .ToList();

            var (transactions, balance) = BuildTransactions(accountId, customer.CreatedAt, histories);
            var currentRate = _fxRateService.GetCurrentRate().Rate;
            var positions = MapPortfoliosToPositions(customer.Portfolios, 100m);

            foreach (var pos in positions)
            {
                pos.CurrentRate = currentRate;
                pos.PnL = Math.Round(CalculatePnL(pos, currentRate), 2);
            }

            var openPnL = positions.Sum(p => p.PnL);
            var margin = positions.Sum(p => p.Margin);
            var equity = balance + openPnL;
            var freeMargin = equity - margin;
            var marginLevel = margin > 0 ? (equity / margin) * 100 : 0;

            return new BalanceSheet
            {
                AccountId = customer.Id,
                AccountNumber = $"FX{10000 + customer.Id}",
                CustomerName = customer.Name,
                AccountType = "Standard",
                Status = "Active",
                Balance = Math.Round(balance, 2),
                Equity = Math.Round(equity, 2),
                OpenPnL = Math.Round(openPnL, 2),
                Margin = Math.Round(margin, 2),
                FreeMargin = Math.Round(freeMargin, 2),
                MarginLevel = margin > 0 ? Math.Round(marginLevel, 2) : 0,
                OpenPositions = positions.ToList(),
                RecentTransactions = transactions
                    .OrderByDescending(t => t.Timestamp)
                    .Take(10)
                    .ToList()
            };
        }

        public FxTransactionResult ExecuteTrade(int accountId, string type, decimal lots)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<FxDbContext>();

            var customer = db.Customers.Include(c => c.Portfolios).FirstOrDefault(c => c.Id == accountId);
            if (customer == null)
                return new FxTransactionResult { Success = false, Message = "Account not found" };

            if (lots <= 0)
                return new FxTransactionResult { Success = false, Message = "Invalid lots amount" };

            var histories = db.CustomerHistories
                .Where(h => h.CustomerId == accountId)
                .OrderBy(h => h.OpenedAt)
                .ToList();

            var (_, balance) = BuildTransactions(accountId, customer.CreatedAt, histories);
            var currentRate = _fxRateService.GetCurrentRate().Rate;
            const decimal leverage = 100m;
            var margin = CalculateMargin(lots, currentRate, leverage);
            var positions = MapPortfoliosToPositions(customer.Portfolios, leverage);
            var openPnL = positions.Sum(p => CalculatePnL(p, currentRate));
            var equity = balance + openPnL;
            var usedMargin = positions.Sum(p => p.Margin);
            var freeMargin = equity - usedMargin;

            if (margin > freeMargin)
                return new FxTransactionResult { Success = false, Message = "Insufficient margin" };

            db.CustomerPortfolios.Add(new CustomerPortfolio
            {
                CustomerId = accountId,
                CurrencyPair = "AUD/USD",
                Direction = type,
                Amount = lots * 100000m,
                EntryRate = currentRate,
                OpenedAt = DateTime.UtcNow,
                Status = "Open"
            });
            db.SaveChanges();

            return new FxTransactionResult
            {
                Success = true,
                Message = $"{type} {lots} lots AUD/USD at {currentRate:F4} executed successfully",
                Transaction = new FxTransaction
                {
                    Type = type,
                    CurrencyPair = "AUD/USD",
                    Amount = lots,
                    Rate = currentRate
                }
            };
        }

        public FxTransactionResult ClosePosition(int accountId, string positionId)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<FxDbContext>();

            var customer = db.Customers.FirstOrDefault(c => c.Id == accountId);
            if (customer == null)
                return new FxTransactionResult { Success = false, Message = "Account not found" };

            // positionId format is "POS{portfolioId}"
            if (!positionId.StartsWith("POS") || !int.TryParse(positionId.Substring(3), out var portfolioId))
                return new FxTransactionResult { Success = false, Message = "Position not found" };

            var portfolio = db.CustomerPortfolios
                .FirstOrDefault(p => p.Id == portfolioId && p.CustomerId == accountId && p.Status == "Open");
            if (portfolio == null)
                return new FxTransactionResult { Success = false, Message = "Position not found" };

            var lots = portfolio.Amount / 100000m;
            var currentRate = _fxRateService.GetCurrentRate().Rate;
            var units = lots * 100000m;
            var pnl = portfolio.Direction == "Buy"
                ? (currentRate - portfolio.EntryRate) * units
                : (portfolio.EntryRate - currentRate) * units;
            pnl = Math.Round(pnl, 2);

            portfolio.Status = "Closed";

            db.CustomerHistories.Add(new CustomerHistory
            {
                CustomerId = accountId,
                CurrencyPair = portfolio.CurrencyPair,
                Direction = portfolio.Direction,
                Amount = portfolio.Amount,
                EntryRate = portfolio.EntryRate,
                ExitRate = currentRate,
                PnL = pnl,
                OpenedAt = portfolio.OpenedAt,
                ClosedAt = DateTime.UtcNow
            });
            db.SaveChanges();

            var closeType = portfolio.Direction == "Buy" ? "Close Buy" : "Close Sell";

            return new FxTransactionResult
            {
                Success = true,
                Message = $"Position closed. P&L: {(pnl >= 0 ? "+" : "")}{pnl:F2}",
                Transaction = new FxTransaction
                {
                    Type = closeType,
                    CurrencyPair = portfolio.CurrencyPair,
                    Amount = lots,
                    Rate = currentRate
                }
            };
        }

        public void AddLead(LeadNotification lead)
        {
            lock (_lock)
            {
                _leads.Add(lead);
            }
        }

        public List<LeadNotification> GetLeads()
        {
            lock (_lock)
            {
                return _leads.OrderByDescending(l => l.ReceivedAt).ToList();
            }
        }

        public void AddTradeNotification(TradeNotification notification)
        {
            lock (_lock)
            {
                _tradeNotifications.Add(notification);
            }

            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<FxDbContext>();

            var customer = db.Customers.FirstOrDefault(c =>
                c.Name == notification.CustomerName)
                ?? db.Customers.FirstOrDefault();

            if (customer != null)
            {
                var rate = notification.Rate > 0 ? notification.Rate : _fxRateService.GetCurrentRate().Rate;

                db.CustomerHistories.Add(new CustomerHistory
                {
                    CustomerId = customer.Id,
                    CurrencyPair = notification.CurrencyPair,
                    Direction = notification.Direction,
                    Amount = notification.Lots * 100000m,
                    EntryRate = rate,
                    ExitRate = rate,
                    PnL = 0m,
                    OpenedAt = notification.ExecutedAt != default ? notification.ExecutedAt : DateTime.UtcNow,
                    ClosedAt = DateTime.UtcNow,
                    Notes = $"Aurora trade notification: {notification.TransactionId}"
                });
                db.SaveChanges();

                _logger.LogInformation("Trade notification recorded in history for account {AccountId} ({Customer}): {Direction} {Lots} lots {Pair} @ {Rate}",
                    customer.Id, customer.Name, notification.Direction, notification.Lots, notification.CurrencyPair, rate);
            }
        }

        public List<TradeNotification> GetTradeNotifications()
        {
            lock (_lock)
            {
                return _tradeNotifications.OrderByDescending(t => t.ReceivedAt).ToList();
            }
        }

    }
}
