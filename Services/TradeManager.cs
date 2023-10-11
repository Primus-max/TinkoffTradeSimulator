using DromAutoTrader.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows;
using TinkoffTradeSimulator.Data;
using TinkoffTradeSimulator.Models;

namespace TinkoffTradeSimulator.Services
{
    public static class TradeManager
    {
        private static AppContext _db = null;

        static TradeManager()
        {
            #region Инициализация базы данных
            InitializeDatabase();
            #endregion
        }

        public static void ExecuteTrade(string operation, string tickerName, double price, bool subtractVolume, int VolumeTradingTicker)
        {
            // Поиск записи с тем же TickerName в _db.TradeRecordsInfo
            TradeRecordInfo tradeRecordInfo = _db.TradeRecordsInfo.SingleOrDefault(tr => tr.TickerName == tickerName);

            if (tradeRecordInfo == null)
            {
                tradeRecordInfo = new TradeRecordInfo
                {
                    TickerName = tickerName,
                    Price = price,
                    IsBuy = operation == "Покупка",
                    Operation = operation,
                    Volume = subtractVolume ? -VolumeTradingTicker : VolumeTradingTicker
                };

                // Добавляем запись в _db.TradeRecordsInfo
                _db.TradeRecordsInfo.Add(tradeRecordInfo);

                // Сохраняем изменения в базе данных
                _db.SaveChanges();
            }
            else
            {
                // Запись найдена, обновляем Volume, Price и Operation
                tradeRecordInfo.Price = price; // Обновляем цену
                tradeRecordInfo.Operation = operation; // Обновляем тип операции

                if (operation == "Покупка")
                {                

                    tradeRecordInfo.Volume += VolumeTradingTicker;

                    // Используйте контекст базы данных для обновления записи
                    _db.TradeRecordsInfo.Update(tradeRecordInfo);

                    _db.SaveChanges();

                    var asdfasdf = _db.TradeRecordsInfo.ToList();
                }

                if (operation == "Продажа")
                {                  

                    // Если нужно вычесть объем, убедимся, что объем не становится отрицательным
                    if (tradeRecordInfo.Volume >= VolumeTradingTicker)
                    {
                        tradeRecordInfo.Volume -= VolumeTradingTicker;

                        // Если объем стал равным 0, удаляем запись из базы данных
                        _db.TradeRecordsInfo.Update(tradeRecordInfo);

                        // Сохраняем изменения в базе данных
                        _db.SaveChanges();
                    }
                    else
                    {
                        // Обработка ошибки, если объем торговли меньше, чем пытаемся продать
                        MessageBox.Show("Недостаточно объема для продажи", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return; // Выходим из метода, так как объем недостаточный
                    }

                    if (tradeRecordInfo.Volume <= 0)
                    {
                        // Если объем стал равным 0, удаляем запись из базы данных
                        _db.TradeRecordsInfo.Remove(tradeRecordInfo);

                        // Сохраняем изменения в базе данных
                        _db.SaveChanges();
                    }
                }
            }         

            // Создаем новую запись для истории торговли
            HistoricalTradeRecordInfo historicalTradeRecordInfo = new HistoricalTradeRecordInfo
            {
                TickerName = tickerName,
                Price = price,
                Operation = operation,
                Volume = VolumeTradingTicker, // Используем значение из VolumeTradingTicker
                Date = DateTime.Now // Устанавливаем дату
            };

            // Добавляем запись в _db.HistoricalTradeRecordsInfo
            _db.HistoricalTradeRecordsInfo.Add(historicalTradeRecordInfo);

            // Сохраняем изменения в базе данных
            _db.SaveChanges();
        }

        // Вызываемый метод покупки тикера
        public static void BuyTicker(string tickerName, double price, int volumeTradingTicker)
        {
            ExecuteTrade("Покупка", tickerName, price, false, volumeTradingTicker);
        }

        // Вызываемый метод продажи тикера
        public static void SellTicker(string tickerName, double price, int volumeTradingTicker)
        {
            ExecuteTrade("Продажа", tickerName, price, true, volumeTradingTicker);
        }

        // Базы данных
        private static void InitializeDatabase()
        {
            try
            {
                // Экземпляр базы данных
                _db = AppContextFactory.GetInstance();
                // загружаем данные о поставщиках из БД
                _db.HistoricalTradeRecordsInfo.Load();
                _db.TradeRecordsInfo.Load();
            }
            catch (Exception)
            {
                // TODO сделать запись логов
                //Console.WriteLine($"Не удалось инициализировать базу данных: {ex.Message}");
            }
        }
    }
}
