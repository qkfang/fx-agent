using FxWebApi.Models;

namespace FxWebApi.Services
{
    public class FxRateService
    {
        private decimal _currentRate = 0.6550m;
        private readonly Random _random = new Random();
        private Timer? _timer;

        public FxRateService()
        {
            // Start the timer to update the rate every 2 seconds
            _timer = new Timer(UpdateRate, null, TimeSpan.Zero, TimeSpan.FromSeconds(2));
        }

        private void UpdateRate(object? state)
        {
            // Randomize the rate up or down by 0.0001 to 0.0050
            var change = (decimal)(_random.NextDouble() * 0.0050);
            var direction = _random.Next(0, 2) == 0 ? -1 : 1;
            _currentRate += change * direction;

            // Keep the rate in a reasonable range
            if (_currentRate < 0.6000m) _currentRate = 0.6000m;
            if (_currentRate > 0.7000m) _currentRate = 0.7000m;
        }

        public FxRate GetCurrentRate()
        {
            return new FxRate
            {
                CurrencyPair = "AUD/USD",
                Rate = _currentRate,
                Timestamp = DateTime.UtcNow
            };
        }

        public FxTransactionResult ExecuteTransaction(string type, decimal amount)
        {
            var currentRate = GetCurrentRate();
            
            return new FxTransactionResult
            {
                Success = true,
                Message = $"{type} transaction executed successfully",
                Transaction = new FxTransaction
                {
                    Type = type,
                    CurrencyPair = "AUD/USD",
                    Amount = amount,
                    Rate = currentRate.Rate
                }
            };
        }
    }
}
