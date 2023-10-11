using DromAutoTrader.Views;
using System;
using System.Linq;
using System.Windows;
using TinkoffTradeSimulator.Models;

namespace TinkoffTradeSimulator.Services
{
    public static class TradeManager
    {
        private static AppContext _db = null;

        public static void ExecuteTrade(string operation, string tickerName, double price, bool subtractVolume, int VolumeTradingTicker)
        {
            // Получаю информацию о тикере обратно из CodeBehind
            // TODO Разобраться с обнулением TickerInfo или StockInfo
            TickerInfo = LocatorService.Current.TickerInfo;

            bool subtractVolume = operation == "Продажа"; // Проверяем, нужно ли вычитать объем

            // Поиск записи с тем же TickerName в _db.TradeRecordsInfo
            TradeRecordInfo tradeRecordInfo = _db.TradeRecordsInfo.SingleOrDefault(tr => tr.TickerName == tickerName);

            if (tradeRecordInfo != null)
            {
                // Запись найдена, обновляем Volume, Price и Operation
                tradeRecordInfo.Price = price; // Обновляем цену
                tradeRecordInfo.Operation = operation; // Обновляем тип операции

                if (subtractVolume)
                {
                    // Если нужно вычесть объем, убедимся, что объем не становится отрицательным
                    if (tradeRecordInfo.Volume >= VolumeTradingTicker)
                    {
                        tradeRecordInfo.Volume -= VolumeTradingTicker;
                    }
                    else
                    {
                        // Обработка ошибки, если объем торговли меньше, чем пытаемся продать
                        MessageBox.Show("Недостаточно объема для продажи", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return; // Выходим из метода, так как объем недостаточный
                    }
                }
                else
                {
                    tradeRecordInfo.Volume += VolumeTradingTicker; // Пример, можете использовать нужную логику увеличения объема
                }

                if (tradeRecordInfo.Volume == 0)
                {
                    // Если объем стал равным 0, удаляем запись из базы данных
                    _db.TradeRecordsInfo.Remove(tradeRecordInfo);
                }
            }
            else if (subtractVolume)
            {
                // Запись не найдена и мы пытаемся продать, отображаем сообщение об ошибке
                MessageBox.Show("Нет записи для продажи", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else
            {
                // Запись не найдена и мы пытаемся купить, создаем новую запись
                tradeRecordInfo = new TradeRecordInfo
                {
                    TickerName = tickerName,
                    Price = price,
                    IsBuy = operation == "Покупка", // Определение типа операции
                    Operation = operation,
                    Volume = subtractVolume ? -VolumeTradingTicker : VolumeTradingTicker // Устанавливаем объем с учетом операции
                };

                // Добавляем запись в _db.TradeRecordsInfo
                _db.TradeRecordsInfo.Add(tradeRecordInfo);
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

        public static void BuyTicker(string tickerName, double price, int volumeTradingTicker)
        {
            ExecuteTrade("Покупка", tickerName, price, false, volumeTradingTicker);
        }

        public static void SellTicker(string tickerName, double price, int volumeTradingTicker)
        {
            ExecuteTrade("Продажа", tickerName, price, true, volumeTradingTicker);
        }
    }
}
