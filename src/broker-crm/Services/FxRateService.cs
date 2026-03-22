using FxWebApi.Models;

namespace FxWebApi.Services
{
    /// <summary>
    /// Realistic FX market-maker simulation for AUD/USD.
    /// Implements geometric Brownian motion with drift (trend bias),
    /// mean reversion, volatility clustering, and bid/ask spread.
    /// </summary>
    public class FxRateService
    {
        // ── Price state ────────────────────────────────────────────────────────
        private decimal _mid = 0.6550m;
        private decimal _spread = 0.0002m;        // 2 pips default spread
        private readonly object _lock = new();

        // ── Trend / volatility controls ────────────────────────────────────────
        private string _trend = "neutral";        // "up" | "down" | "neutral"
        private double _trendStrength = 0.0;      // -1..+1 (negative = bearish)
        private double _volatility = 0.0004;      // base per-tick sigma
        private double _volAccumulator = 0.0;     // GARCH-style vol clustering
        private readonly double _meanReversionSpeed = 0.005;
        private readonly decimal _meanPrice = 0.6550m;

        // ── Day-session stats ──────────────────────────────────────────────────
        private decimal _dayOpen;
        private decimal _dayHigh;
        private decimal _dayLow;

        // ── OHLC candle builder (1-minute) ─────────────────────────────────────
        private readonly List<OhlcCandle> _candles = new();
        private OhlcCandle _currentCandle;
        private DateTime _candleStart;
        private readonly TimeSpan _candleInterval = TimeSpan.FromSeconds(30);

        // ── Transaction log ───────────────────────────────────────────────────
        private readonly List<TransactionRecord> _transactions = new();
        private int _nextTxId = 1;

        // ── Tick timer ────────────────────────────────────────────────────────
        private readonly Random _rng = new();
        private Timer? _timer;

        public FxRateService()
        {
            _dayOpen = _dayHigh = _dayLow = _mid;
            _candleStart = DateTime.UtcNow;
            _currentCandle = NewCandle(_candleStart, _mid);

            // Seed 200 synthetic historical candles so the chart has data immediately
            SeedHistory();

            // Tick every 1 second
            _timer = new Timer(Tick, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
        }

        // ── Public API ────────────────────────────────────────────────────────

        public FxRate GetCurrentRate()
        {
            lock (_lock)
            {
                return new FxRate
                {
                    CurrencyPair = "AUD/USD",
                    Rate = _mid,
                    Timestamp = DateTime.UtcNow
                };
            }
        }

        public FxQuote GetCurrentQuote()
        {
            lock (_lock)
            {
                var halfSpread = _spread / 2;
                var bid = Math.Round(_mid - halfSpread, 4);
                var ask = Math.Round(_mid + halfSpread, 4);
                return new FxQuote
                {
                    CurrencyPair = "AUD/USD",
                    Bid = bid,
                    Ask = ask,
                    Mid = Math.Round(_mid, 4),
                    Spread = Math.Round(_spread, 4),
                    SpreadPips = (int)Math.Round(_spread * 10000),
                    Timestamp = DateTime.UtcNow
                };
            }
        }

        public List<OhlcCandle> GetHistory(int bars = 100)
        {
            lock (_lock)
            {
                var all = new List<OhlcCandle>(_candles) { _currentCandle };
                return all.TakeLast(Math.Min(bars, all.Count)).ToList();
            }
        }

        public OrderBook GetOrderBook()
        {
            lock (_lock)
            {
                var halfSpread = _spread / 2;
                var bid = Math.Round(_mid - halfSpread, 4);
                var ask = Math.Round(_mid + halfSpread, 4);

                var bids = new List<OrderBookLevel>();
                var asks = new List<OrderBookLevel>();
                decimal cumBid = 0, cumAsk = 0;

                for (int i = 0; i < 8; i++)
                {
                    // Liquidity increases away from top-of-book
                    var sz = Math.Round((decimal)(500_000 * (1.0 + i * 0.5) * (0.8 + _rng.NextDouble() * 0.4)), 0);
                    cumBid += sz;
                    bids.Add(new OrderBookLevel
                    {
                        Price = Math.Round(bid - i * 0.0001m, 4),
                        Size = sz,
                        Total = cumBid
                    });

                    var szA = Math.Round((decimal)(500_000 * (1.0 + i * 0.5) * (0.8 + _rng.NextDouble() * 0.4)), 0);
                    cumAsk += szA;
                    asks.Add(new OrderBookLevel
                    {
                        Price = Math.Round(ask + i * 0.0001m, 4),
                        Size = szA,
                        Total = cumAsk
                    });
                }

                return new OrderBook
                {
                    CurrencyPair = "AUD/USD",
                    Bids = bids,
                    Asks = asks,
                    Timestamp = DateTime.UtcNow
                };
            }
        }

        public MarketStatus GetMarketStatus()
        {
            lock (_lock)
            {
                var change = _mid - _dayOpen;
                var pct = _dayOpen == 0 ? 0 : Math.Round(change / _dayOpen * 100, 3);
                var session = GetTradingSession();
                return new MarketStatus
                {
                    Trend = _trend.Substring(0, 1).ToUpper() + _trend.Substring(1),
                    Volatility = Math.Round(_volatility * 10000, 2), // expressed as pips
                    DayOpen = Math.Round(_dayOpen, 4),
                    DayHigh = Math.Round(_dayHigh, 4),
                    DayLow = Math.Round(_dayLow, 4),
                    DayChange = Math.Round(change, 4),
                    DayChangePct = pct,
                    Session = session
                };
            }
        }

        public void SetTrend(string direction, int strength)
        {
            lock (_lock)
            {
                _trend = direction.ToLower() switch
                {
                    "up" => "up",
                    "down" => "down",
                    _ => "neutral"
                };
                // Normalise strength to a drift factor
                _trendStrength = direction.ToLower() switch
                {
                    "up" => strength / 100.0,
                    "down" => -(strength / 100.0),
                    _ => 0.0
                };
                // Raise volatility when a trend is triggered
                if (_trend != "neutral")
                    _volatility = 0.0006 + (strength / 100.0) * 0.0006;
            }
        }

        public FxTransactionResult ExecuteTransaction(string type, decimal amount, string source = "API")
        {
            FxQuote quote;
            lock (_lock) { quote = GetCurrentQuote(); }

            // Buy at ask, sell at bid (market maker convention)
            var executionRate = type.ToLower() == "buy" ? quote.Ask : quote.Bid;
            var total = Math.Round(amount * executionRate, 2);

            var record = new TransactionRecord
            {
                Id = $"TXN{_nextTxId++:D6}",
                Type = type,
                CurrencyPair = "AUD/USD",
                Amount = amount,
                Rate = executionRate,
                Total = total,
                Source = source,
                Timestamp = DateTime.UtcNow
            };

            lock (_lock) { _transactions.Insert(0, record); }

            return new FxTransactionResult
            {
                Success = true,
                Message = $"{type} {amount:N0} AUD/USD executed at {executionRate:F4}",
                Transaction = new FxTransaction
                {
                    Type = type,
                    CurrencyPair = "AUD/USD",
                    Amount = amount,
                    Rate = executionRate
                },
                Record = record
            };
        }

        public List<TransactionRecord> GetTransactions(int limit = 50)
        {
            lock (_lock)
            {
                return _transactions.Take(limit).ToList();
            }
        }

        // ── Internal price engine ─────────────────────────────────────────────

        private void Tick(object? state)
        {
            lock (_lock)
            {
                // ── 1. Geometric Brownian Motion with trend drift ──────────────
                // GARCH(1,1) style volatility update
                _volAccumulator = 0.85 * _volAccumulator + 0.15 * _volatility;
                double sigma = Math.Max(0.0001, Math.Sqrt(_volAccumulator * _volAccumulator + _volatility * _volatility));

                // Box-Muller normal sample
                double u1 = 1.0 - _rng.NextDouble();
                double u2 = 1.0 - _rng.NextDouble();
                double z = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);

                // Drift: trend bias + mean reversion
                double meanRev = _meanReversionSpeed * (double)(_meanPrice - _mid);
                double drift = _trendStrength * 0.0003 + meanRev;

                var tick = (decimal)(drift + sigma * z);
                _mid += tick;

                // Hard bounds
                _mid = Math.Max(0.5800m, Math.Min(0.7500m, _mid));

                // ── 2. Adaptive spread (widen on high vol) ─────────────────────
                _spread = (decimal)Math.Round(0.0002 + sigma * 0.8, 4);
                _spread = Math.Max(0.0001m, Math.Min(0.0010m, _spread));

                // ── 3. Day stats ───────────────────────────────────────────────
                if (_mid > _dayHigh) _dayHigh = _mid;
                if (_mid < _dayLow) _dayLow = _mid;

                // ── 4. OHLC candle building ────────────────────────────────────
                _currentCandle.High = Math.Max(_currentCandle.High, _mid);
                _currentCandle.Low = Math.Min(_currentCandle.Low, _mid);
                _currentCandle.Close = _mid;
                _currentCandle.Volume += (long)(_rng.Next(100_000, 2_000_000));

                if (DateTime.UtcNow - _candleStart >= _candleInterval)
                {
                    _candles.Add(_currentCandle);
                    if (_candles.Count > 500) _candles.RemoveAt(0);
                    _candleStart = DateTime.UtcNow;
                    _currentCandle = NewCandle(_candleStart, _mid);
                }

                // Slowly decay trend strength back toward neutral
                _trendStrength *= 0.998;
                if (Math.Abs(_trendStrength) < 0.01) _trendStrength = 0.0;
            }
        }

        private void SeedHistory()
        {
            // Walk backwards 200 candles from the start time
            var price = _mid;
            var baseVol = 0.0003;
            var seedTime = DateTime.UtcNow.AddSeconds(-(200 * _candleInterval.TotalSeconds));
            var rng = new Random(42);

            for (int i = 0; i < 200; i++)
            {
                double u1 = 1.0 - rng.NextDouble();
                double u2 = 1.0 - rng.NextDouble();
                double z = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
                decimal o = Math.Round(price, 4);
                price += (decimal)(baseVol * z);
                price = Math.Max(0.5800m, Math.Min(0.7500m, price));
                decimal c = Math.Round(price, 4);
                decimal hi = Math.Max(o, c) + (decimal)(rng.NextDouble() * 0.0003);
                decimal lo = Math.Min(o, c) - (decimal)(rng.NextDouble() * 0.0003);

                _candles.Add(new OhlcCandle
                {
                    Timestamp = seedTime.AddSeconds(i * _candleInterval.TotalSeconds),
                    Open = o,
                    High = Math.Round(hi, 4),
                    Low = Math.Round(lo, 4),
                    Close = c,
                    Volume = rng.Next(200_000, 5_000_000)
                });
            }
            _mid = price;
            _dayOpen = _dayHigh = _dayLow = _mid;
        }

        private static OhlcCandle NewCandle(DateTime ts, decimal price) =>
            new() { Timestamp = ts, Open = price, High = price, Low = price, Close = price, Volume = 0 };

        private static string GetTradingSession()
        {
            var hour = DateTime.UtcNow.Hour;
            if (hour >= 21 || hour < 6)  return "Sydney / Tokyo";
            if (hour < 8)                return "Tokyo / London";
            if (hour < 13)               return "London";
            if (hour < 17)               return "London / New York";
            return "New York";
        }
    }
}
