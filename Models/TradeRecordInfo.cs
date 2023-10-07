using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinkoffTradeSimulator.Models.Interfaces;

namespace TinkoffTradeSimulator.Models
{
    public class TradeRecordInfo : ITradeInfo
    {
        public int Id { get; set; } 
        public DateTime Date { get; set; } = DateTime.Now;// Дата и время сделки
        public string? TickerName { get; set; } = string.Empty; // Название инструмента
        public double Price { get; set; } = 0; // Цена
        public string? Operation { get; set; } = string.Empty;// Тип операции (покупка/продажа и т. д.)
        public int Volume { get; set; } = 0; // Объем сделки
        public bool IsBuy { get; set; } = false; // Покупка
        public bool IsSell { get; set; } = false; // Продажа
        public bool IsClosed { get; set; } = false; // Закрытая / открытая сделка
    }
}

