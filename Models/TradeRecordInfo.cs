using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinkoffTradeSimulator.Models
{
    public class TradeRecordInfo
    {
        public int Id { get; set; } // Уникальный идентификатор записи
        public DateTime Date { get; set; } = DateTime.Now;// Дата и время сделки
        public string? TickerName { get; set; } = string.Empty; // Название инструмента
        public double Price { get; set; } = 0; // Цена
        public string? Operation { get; set; } = string.Empty;// Тип операции (покупка/продажа и т. д.)
        public int Volume { get; set; } = 0; // Объем сделки
        public bool IsBuy { get; set; } = false; // Куплена
        public bool IsSell { get; set; } = false; // Продана
    }
}

