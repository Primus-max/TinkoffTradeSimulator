using System;
using System.Collections.ObjectModel;
using TinkoffTradeSimulator.Models;

namespace TinkoffTradeSimulator.Services
{
    public class EventAggregator : EventArgs
    {
        public static event Action<CandleTimeFrameButton>? CandleIntervalSelected;

        public static void PublishCandleIntervalSelected(CandleTimeFrameButton selectedButton)
        {
            CandleIntervalSelected?.Invoke(selectedButton);
        }

        public static event Action<ObservableCollection<HistoricalTradeRecordInfo>>? HistoricalTradeInfoChanged;

        public static void PublishHistoricalTradeInfoChanged(ObservableCollection<HistoricalTradeRecordInfo> historicalTradeRecordInfo)
        {
            HistoricalTradeInfoChanged?.Invoke(historicalTradeRecordInfo);
        }

        public static event Action<ObservableCollection<TradeRecordInfo>>? TradingRecordInfoChanged;

        public static void PublishTradingInfoChanged(ObservableCollection<TradeRecordInfo> tradingRecordInfo)
        {
            TradingRecordInfoChanged?.Invoke(tradingRecordInfo);
        }
    }
}