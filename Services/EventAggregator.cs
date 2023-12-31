﻿using System;
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
              
        // Событие обновления источников данных. Продажа / покупка / закрытие сделки
        public static event Action? TradingRecordInfoChanged;

        public static void PublishTradingInfoChanged()
        {
            TradingRecordInfoChanged?.Invoke();
        }

        // Событие, которое будет вызываться при запросе обновления данных
        public static event Action? UpdateDataRequested;

        // Метод для опубликования запроса на обновление данных
        public static void PublishUpdateDataRequested()
        {
            UpdateDataRequested?.Invoke();
        }

        public static event Action<TickerInfo>? UpdateTickerInfo;

        public static void PublishUpdateTickerInfo(TickerInfo stockInfo)
        {           
            UpdateTickerInfo?.Invoke(stockInfo);
        }       
    }
}