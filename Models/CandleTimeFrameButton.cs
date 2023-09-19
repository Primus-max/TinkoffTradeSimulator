using System;
using Tinkoff.InvestApi.V1;

namespace TinkoffTradeSimulator.Models
{
    public class CandleTimeFrameButton
    {
        public string? Name { get; set; }
        public TimeSpan Time { get; set; }
    }
}
