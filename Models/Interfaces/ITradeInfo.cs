using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinkoffTradeSimulator.Models.Interfaces
{
    internal interface ITradeInfo
    {
        int Id { get; set; }
        DateTime Date { get; set; }
        string TickerName { get; set; }
        double Price { get; set; }
        string Operation { get; set; }
        int Volume { get; set; }
        bool IsBuy { get; set; }
        bool IsSell { get; set; }
        bool IsClosed { get; set; }
    }
}
