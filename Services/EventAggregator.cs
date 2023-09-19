using System;
using Tinkoff.InvestApi.V1;
using TinkoffTradeSimulator.Models;

public class EventAggregator
{
    public static event Action<CandleTimeFrameButton>? CandleIntervalSelected;

    public static void PublishCandleIntervalSelected(CandleTimeFrameButton selectedButton)
    {
        CandleIntervalSelected?.Invoke(selectedButton);
    }
}
