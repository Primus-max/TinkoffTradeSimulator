using System;

namespace TinkoffTradeSimulator.Utils
{
    public class ChartToolTipManager
    {
        public string Time { get; set; }
        public string Price { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }

        public ChartToolTipManager(string time, string price, double width, double height)
        {
            Time = time;
            Price = price;
            Width = width;
            Height = height;
        }
    }
}
