using FxWebApi.Models;
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
        private readonly List<Account> _accounts;
        private readonly Dictionary<int, List<Position>> _positions;
        private readonly Dictionary<int, List<AccountTransaction>> _transactions;
        private readonly List<LeadNotification> _leads = new();
        private readonly object _lock = new();

        public AccountService(FxRateService fxRateService, IHttpClientFactory httpClientFactory,
            IConfiguration config, ILogger<AccountService> logger)
        {
            _fxRateService = fxRateService;
            _httpClientFactory = httpClientFactory;
            _config = config;
            _logger = logger;

            _accounts = new List<Account>
            {
                new Account
                {
                    Id = 1,
                    AccountNumber = "FX10001",
                    CustomerName = "James Wilson",
                    Email = "james.wilson@email.com",
                    Country = "Australia",
                    AccountType = "Standard",
                    Status = "Active",
                    Balance = 50125m,
                    Leverage = 100m,
                    CreatedAt = new DateTime(2023, 3, 15)
                },
                new Account
                {
                    Id = 2,
                    AccountNumber = "FX10002",
                    CustomerName = "Sarah Chen",
                    Email = "sarah.chen@email.com",
                    Country = "Singapore",
                    AccountType = "Professional",
                    Status = "Active",
                    Balance = 126375m,
                    Leverage = 200m,
                    CreatedAt = new DateTime(2022, 11, 8)
                },
                new Account
                {
                    Id = 3,
                    AccountNumber = "FX10003",
                    CustomerName = "Michael Torres",
                    Email = "m.torres@email.com",
                    Country = "United States",
                    AccountType = "VIP",
                    Status = "Active",
                    Balance = 74600m,
                    Leverage = 100m,
                    CreatedAt = new DateTime(2024, 1, 20)
                }
            };

            _positions = new Dictionary<int, List<Position>>
            {
                {
                    1, new List<Position>
                    {
                        new Position
                        {
                            PositionId = "POS001",
                            AccountId = 1,
                            CurrencyPair = "AUD/USD",
                            Type = "Buy",
                            Lots = 0.5m,
                            OpenRate = 0.6520m,
                            CurrentRate = 0.6520m,
                            PnL = 0m,
                            Margin = 326m,
                            OpenTime = DateTime.UtcNow.AddHours(-3)
                        }
                    }
                },
                {
                    2, new List<Position>
                    {
                        new Position
                        {
                            PositionId = "POS002",
                            AccountId = 2,
                            CurrencyPair = "AUD/USD",
                            Type = "Sell",
                            Lots = 1.0m,
                            OpenRate = 0.6580m,
                            CurrentRate = 0.6580m,
                            PnL = 0m,
                            Margin = 329m,
                            OpenTime = DateTime.UtcNow.AddHours(-6)
                        },
                        new Position
                        {
                            PositionId = "POS003",
                            AccountId = 2,
                            CurrencyPair = "AUD/USD",
                            Type = "Buy",
                            Lots = 0.3m,
                            OpenRate = 0.6540m,
                            CurrentRate = 0.6540m,
                            PnL = 0m,
                            Margin = 98.1m,
                            OpenTime = DateTime.UtcNow.AddHours(-2)
                        }
                    }
                },
                { 3, new List<Position>() }
            };

            var now = DateTime.UtcNow;
            _transactions = new Dictionary<int, List<AccountTransaction>>
            {
                {
                    1, new List<AccountTransaction>
                    {
                        new AccountTransaction { TransactionId = "T001", AccountId = 1, Type = "Deposit", CurrencyPair = "-", Lots = 0, Rate = 0, PnL = 0, BalanceAfter = 50000m, Timestamp = new DateTime(2023, 3, 15) },
                        new AccountTransaction { TransactionId = "T002", AccountId = 1, Type = "Buy", CurrencyPair = "AUD/USD", Lots = 1.0m, Rate = 0.6485m, PnL = 0, BalanceAfter = 50000m, Timestamp = now.AddDays(-5) },
                        new AccountTransaction { TransactionId = "T003", AccountId = 1, Type = "Close Buy", CurrencyPair = "AUD/USD", Lots = 1.0m, Rate = 0.6530m, PnL = 450m, BalanceAfter = 50450m, Timestamp = now.AddDays(-4) },
                        new AccountTransaction { TransactionId = "T004", AccountId = 1, Type = "Sell", CurrencyPair = "AUD/USD", Lots = 0.5m, Rate = 0.6555m, PnL = 0, BalanceAfter = 50450m, Timestamp = now.AddDays(-3) },
                        new AccountTransaction { TransactionId = "T005", AccountId = 1, Type = "Close Sell", CurrencyPair = "AUD/USD", Lots = 0.5m, Rate = 0.6510m, PnL = 225m, BalanceAfter = 50675m, Timestamp = now.AddDays(-2) },
                        new AccountTransaction { TransactionId = "T006", AccountId = 1, Type = "Sell", CurrencyPair = "AUD/USD", Lots = 1.0m, Rate = 0.6590m, PnL = 0, BalanceAfter = 50675m, Timestamp = now.AddDays(-1) },
                        new AccountTransaction { TransactionId = "T007", AccountId = 1, Type = "Close Sell", CurrencyPair = "AUD/USD", Lots = 1.0m, Rate = 0.6645m, PnL = -550m, BalanceAfter = 50125m, Timestamp = now.AddHours(-12) },
                        new AccountTransaction { TransactionId = "T008", AccountId = 1, Type = "Buy", CurrencyPair = "AUD/USD", Lots = 0.5m, Rate = 0.6520m, PnL = 0, BalanceAfter = 50125m, Timestamp = now.AddHours(-3) }
                    }
                },
                {
                    2, new List<AccountTransaction>
                    {
                        new AccountTransaction { TransactionId = "T101", AccountId = 2, Type = "Deposit", CurrencyPair = "-", Lots = 0, Rate = 0, PnL = 0, BalanceAfter = 125000m, Timestamp = new DateTime(2022, 11, 8) },
                        new AccountTransaction { TransactionId = "T102", AccountId = 2, Type = "Buy", CurrencyPair = "AUD/USD", Lots = 2.0m, Rate = 0.6430m, PnL = 0, BalanceAfter = 125000m, Timestamp = now.AddDays(-7) },
                        new AccountTransaction { TransactionId = "T103", AccountId = 2, Type = "Close Buy", CurrencyPair = "AUD/USD", Lots = 2.0m, Rate = 0.6510m, PnL = 1600m, BalanceAfter = 126600m, Timestamp = now.AddDays(-6) },
                        new AccountTransaction { TransactionId = "T104", AccountId = 2, Type = "Sell", CurrencyPair = "AUD/USD", Lots = 1.5m, Rate = 0.6600m, PnL = 0, BalanceAfter = 126600m, Timestamp = now.AddDays(-5) },
                        new AccountTransaction { TransactionId = "T105", AccountId = 2, Type = "Close Sell", CurrencyPair = "AUD/USD", Lots = 1.5m, Rate = 0.6545m, PnL = 825m, BalanceAfter = 127425m, Timestamp = now.AddDays(-4) },
                        new AccountTransaction { TransactionId = "T106", AccountId = 2, Type = "Buy", CurrencyPair = "AUD/USD", Lots = 3.0m, Rate = 0.6490m, PnL = 0, BalanceAfter = 127425m, Timestamp = now.AddDays(-3) },
                        new AccountTransaction { TransactionId = "T107", AccountId = 2, Type = "Close Buy", CurrencyPair = "AUD/USD", Lots = 3.0m, Rate = 0.6455m, PnL = -1050m, BalanceAfter = 126375m, Timestamp = now.AddDays(-2) },
                        new AccountTransaction { TransactionId = "T108", AccountId = 2, Type = "Sell", CurrencyPair = "AUD/USD", Lots = 1.0m, Rate = 0.6580m, PnL = 0, BalanceAfter = 126375m, Timestamp = now.AddHours(-6) },
                        new AccountTransaction { TransactionId = "T109", AccountId = 2, Type = "Buy", CurrencyPair = "AUD/USD", Lots = 0.3m, Rate = 0.6540m, PnL = 0, BalanceAfter = 126375m, Timestamp = now.AddHours(-2) }
                    }
                },
                {
                    3, new List<AccountTransaction>
                    {
                        new AccountTransaction { TransactionId = "T201", AccountId = 3, Type = "Deposit", CurrencyPair = "-", Lots = 0, Rate = 0, PnL = 0, BalanceAfter = 75000m, Timestamp = new DateTime(2024, 1, 20) },
                        new AccountTransaction { TransactionId = "T202", AccountId = 3, Type = "Buy", CurrencyPair = "AUD/USD", Lots = 1.0m, Rate = 0.6510m, PnL = 0, BalanceAfter = 75000m, Timestamp = now.AddDays(-4) },
                        new AccountTransaction { TransactionId = "T203", AccountId = 3, Type = "Close Buy", CurrencyPair = "AUD/USD", Lots = 1.0m, Rate = 0.6560m, PnL = 500m, BalanceAfter = 75500m, Timestamp = now.AddDays(-3) },
                        new AccountTransaction { TransactionId = "T204", AccountId = 3, Type = "Sell", CurrencyPair = "AUD/USD", Lots = 0.5m, Rate = 0.6570m, PnL = 0, BalanceAfter = 75500m, Timestamp = now.AddDays(-2) },
                        new AccountTransaction { TransactionId = "T205", AccountId = 3, Type = "Close Sell", CurrencyPair = "AUD/USD", Lots = 0.5m, Rate = 0.6530m, PnL = 200m, BalanceAfter = 75700m, Timestamp = now.AddDays(-1) },
                        new AccountTransaction { TransactionId = "T206", AccountId = 3, Type = "Buy", CurrencyPair = "AUD/USD", Lots = 2.0m, Rate = 0.6545m, PnL = 0, BalanceAfter = 75700m, Timestamp = now.AddHours(-5) },
                        new AccountTransaction { TransactionId = "T207", AccountId = 3, Type = "Close Buy", CurrencyPair = "AUD/USD", Lots = 2.0m, Rate = 0.6490m, PnL = -1100m, BalanceAfter = 74600m, Timestamp = now.AddHours(-3) }
                    }
                }
            };
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

        public List<AccountSummary> GetAllAccounts()
        {
            lock (_lock)
            {
                var currentRate = _fxRateService.GetCurrentRate().Rate;
                var summaries = new List<AccountSummary>();

                foreach (var account in _accounts)
                {
                    var positions = _positions[account.Id];
                    var openPnL = positions.Sum(p => CalculatePnL(p, currentRate));

                    summaries.Add(new AccountSummary
                    {
                        Id = account.Id,
                        AccountNumber = account.AccountNumber,
                        CustomerName = account.CustomerName,
                        AccountType = account.AccountType,
                        Status = account.Status,
                        Country = account.Country,
                        Balance = account.Balance,
                        Equity = Math.Round(account.Balance + openPnL, 2),
                        OpenPnL = Math.Round(openPnL, 2),
                        OpenPositionsCount = positions.Count,
                        Leverage = account.Leverage,
                        CreatedAt = account.CreatedAt
                    });
                }

                return summaries;
            }
        }

        public BalanceSheet? GetBalanceSheet(int accountId)
        {
            lock (_lock)
            {
                var account = _accounts.FirstOrDefault(a => a.Id == accountId);
                if (account == null) return null;

                var currentRate = _fxRateService.GetCurrentRate().Rate;
                var positions = _positions[accountId];

                foreach (var pos in positions)
                {
                    pos.CurrentRate = currentRate;
                    pos.PnL = Math.Round(CalculatePnL(pos, currentRate), 2);
                }

                var openPnL = positions.Sum(p => p.PnL);
                var margin = positions.Sum(p => p.Margin);
                var equity = account.Balance + openPnL;
                var freeMargin = equity - margin;
                var marginLevel = margin > 0 ? (equity / margin) * 100 : 0;

                return new BalanceSheet
                {
                    AccountId = account.Id,
                    AccountNumber = account.AccountNumber,
                    CustomerName = account.CustomerName,
                    AccountType = account.AccountType,
                    Status = account.Status,
                    Balance = Math.Round(account.Balance, 2),
                    Equity = Math.Round(equity, 2),
                    OpenPnL = Math.Round(openPnL, 2),
                    Margin = Math.Round(margin, 2),
                    FreeMargin = Math.Round(freeMargin, 2),
                    MarginLevel = margin > 0 ? Math.Round(marginLevel, 2) : 0,
                    OpenPositions = positions.ToList(),
                    RecentTransactions = _transactions[accountId]
                        .OrderByDescending(t => t.Timestamp)
                        .Take(10)
                        .ToList()
                };
            }
        }

        public FxTransactionResult ExecuteTrade(int accountId, string type, decimal lots)
        {
            lock (_lock)
            {
                var account = _accounts.FirstOrDefault(a => a.Id == accountId);
                if (account == null)
                    return new FxTransactionResult { Success = false, Message = "Account not found" };

                if (lots <= 0)
                    return new FxTransactionResult { Success = false, Message = "Invalid lots amount" };

                var currentRate = _fxRateService.GetCurrentRate().Rate;
                var margin = CalculateMargin(lots, currentRate, account.Leverage);
                var openPnL = _positions[accountId].Sum(p => CalculatePnL(p, currentRate));
                var equity = account.Balance + openPnL;
                var usedMargin = _positions[accountId].Sum(p => p.Margin);
                var freeMargin = equity - usedMargin;

                if (margin > freeMargin)
                    return new FxTransactionResult { Success = false, Message = "Insufficient margin" };

                var position = new Position
                {
                    PositionId = $"POS{DateTime.UtcNow.Ticks}",
                    AccountId = accountId,
                    CurrencyPair = "AUD/USD",
                    Type = type,
                    Lots = lots,
                    OpenRate = currentRate,
                    CurrentRate = currentRate,
                    PnL = 0m,
                    Margin = Math.Round(margin, 2),
                    OpenTime = DateTime.UtcNow
                };

                _positions[accountId].Add(position);

                _transactions[accountId].Add(new AccountTransaction
                {
                    TransactionId = $"T{DateTime.UtcNow.Ticks}",
                    AccountId = accountId,
                    Type = type,
                    CurrencyPair = "AUD/USD",
                    Lots = lots,
                    Rate = currentRate,
                    PnL = 0,
                    BalanceAfter = account.Balance,
                    Timestamp = DateTime.UtcNow
                });

                var result = new FxTransactionResult
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

                // Fire-and-forget settlement to Trading Platform
                var notionalAmount = lots * 100000m;
                NotifyTradingPlatformAsync(type, "AUD/USD", notionalAmount, currentRate);

                return result;
            }
        }

        public FxTransactionResult ClosePosition(int accountId, string positionId)
        {
            lock (_lock)
            {
                var account = _accounts.FirstOrDefault(a => a.Id == accountId);
                if (account == null)
                    return new FxTransactionResult { Success = false, Message = "Account not found" };

                var position = _positions[accountId].FirstOrDefault(p => p.PositionId == positionId);
                if (position == null)
                    return new FxTransactionResult { Success = false, Message = "Position not found" };

                var currentRate = _fxRateService.GetCurrentRate().Rate;
                var pnl = Math.Round(CalculatePnL(position, currentRate), 2);

                _positions[accountId].Remove(position);
                account.Balance += pnl;

                var closeType = position.Type == "Buy" ? "Close Buy" : "Close Sell";
                _transactions[accountId].Add(new AccountTransaction
                {
                    TransactionId = $"T{DateTime.UtcNow.Ticks}",
                    AccountId = accountId,
                    Type = closeType,
                    CurrencyPair = "AUD/USD",
                    Lots = position.Lots,
                    Rate = currentRate,
                    PnL = pnl,
                    BalanceAfter = Math.Round(account.Balance, 2),
                    Timestamp = DateTime.UtcNow
                });

                return new FxTransactionResult
                {
                    Success = true,
                    Message = $"Position closed. P&L: {(pnl >= 0 ? "+" : "")}{pnl:F2}",
                    Transaction = new FxTransaction
                    {
                        Type = closeType,
                        CurrencyPair = "AUD/USD",
                        Amount = position.Lots,
                        Rate = currentRate
                    }
                };
            }
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

        private void NotifyTradingPlatformAsync(string type, string currencyPair, decimal amount, decimal rate)
        {
            var tradingPlatformUrl = _config["TradingPlatformUrl"];
            if (string.IsNullOrWhiteSpace(tradingPlatformUrl)) return;

            var settlement = new TradeSettlementRequest
            {
                Type = type,
                CurrencyPair = currencyPair,
                Amount = amount,
                Rate = rate,
                Total = Math.Round(amount * rate, 2),
                Source = "BrokerCRM",
                DateTime = DateTime.UtcNow
            };

            _ = Task.Run(async () =>
            {
                try
                {
                    var client = _httpClientFactory.CreateClient();
                    var json = JsonSerializer.Serialize(settlement);
                    await client.PostAsync(
                        $"{tradingPlatformUrl}/api/trades",
                        new StringContent(json, Encoding.UTF8, "application/json"));
                }
                catch (Exception ex)
                {
                    // TODO: For production resilience, implement retry-with-backoff or a
                    // dead-letter queue here to guarantee at-least-once delivery of trade
                    // settlements to the Trading Platform.
                    _logger.LogWarning(ex, "Failed to notify trading platform of trade settlement ({Type} {Amount} {Pair})",
                        type, amount, currencyPair);
                }
            });
        }
    }
}
