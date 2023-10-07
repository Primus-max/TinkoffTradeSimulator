using TinkoffTradeSimulator.Models;

namespace DromAutoTrader.Views
{
    internal class LocatorService
    {

        public TickerInfo? TickerInfo { get; set; } = null!;

        public static LocatorService Current { get; } = new LocatorService();
    }
}
