using Google.Protobuf.WellKnownTypes;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using Tinkoff.InvestApi;
using Tinkoff.InvestApi.V1;

namespace TinkoffTradeSimulator.ApiServices.Tinkoff
{
    class TinkoffTradingPrices
    {
        private static InvestApiClient? _client = null;
        private static string _currentTicker = string.Empty;
        private static double _currentCandleHistoricalIntervalIndex = 10; 

        public TinkoffTradingPrices(InvestApiClient client)
        {

            // Создаю клиента
            LoadClientAsync();
        }

        private static async void LoadClientAsync()
        {
            // Создаю клиента Тинькофф 
            _client = await TinkoffClient.CreateAsync();            
        }

        // Получае Share по тикеру
        public static async Task<Share> GetShareByTicker(string ticker)
        {
            Share share = new();
            try
            {
                // Получаю инструмент 
                SharesResponse sharesResponse = await _client?.Instruments.SharesAsync();
                share = sharesResponse?.Instruments?.FirstOrDefault(x => x.Ticker == ticker) ?? new Share();
            }
            catch (Exception )
            {
                //MessageBox.Show($"Не удалось получить инструмент. Причина: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return share;
        }

        // Получаю исторические свечи по Share и временному промежутку
        public static async Task<List<HistoricCandle>> GetCandles(Share instrument, TimeSpan timeFrame, CandleInterval candleIndexInterval)
        {            
            DateTimeOffset now = DateTimeOffset.Now;
            DateTimeOffset intervalAgo = now.Subtract(timeFrame);
            Timestamp nowTimestamp = Timestamp.FromDateTimeOffset(now);
            Timestamp intervalAgoTimestamp = Timestamp.FromDateTimeOffset(intervalAgo);

            // Формирую объект для отправки на сервер
            var request = new GetCandlesRequest()
            {
                InstrumentId = instrument.Uid,
                From = intervalAgoTimestamp,
                To = nowTimestamp,
                Interval = candleIndexInterval
            };

            try
            {
                // Отправляю запрос и получаю ответ по свечам
                var response = await _client?.MarketData.GetCandlesAsync(request);

                List<HistoricCandle>? candles = response.Candles?.ToList();

                return candles;
            }
            catch (Exception)
            {
                // Обработка ошибки при получении свечей
                return new List<HistoricCandle>();
            }
        }

        // Метод обновления и получения данных по свечам, учитывая имя тикера, таймфрем, временной интервал 
        public static async Task<OHLC[]> GetCandlesData(string ticker = null, int? candleHistoricalIntervalIndex = null, CandleInterval? candleInterval = null)
        {
            
            // Обновляем текущие значения, если параметры были переданы
            if (ticker != null)
            {
                _currentTicker = ticker;
            }

            if (candleHistoricalIntervalIndex != null)
            {
                _currentCandleHistoricalIntervalIndex = candleHistoricalIntervalIndex.Value;
            }            

            try
            {
                TinkoffTradingPrices tinkoff = new(_client);
                Share instrument = await GetShareByTicker(_currentTicker);

                // Определяем временной интервал для запроса свечей
                TimeSpan timeFrame = TimeSpan.FromMinutes(_currentCandleHistoricalIntervalIndex);

                // Определение CandleInterval на основе параметра или значения по умолчанию
                CandleInterval interval = candleInterval ?? CandleInterval._1Min;

                List<HistoricCandle> candles = await GetCandles(instrument, timeFrame, interval);

                List<OHLC> prices = new List<OHLC>();

                foreach (var candle in candles)
                {
                    // Преобразовываем данные в OHLC
                    double openPriceCandle = Convert.ToDouble(candle.Open);
                    double highPriceCandle = Convert.ToDouble(candle.High);
                    double lowPriceCandle = Convert.ToDouble(candle.Low);
                    double closePriceCandle = Convert.ToDouble(candle.Close);

                    // Преобразуем Timestamp в DateTime
                    DateTime candleTime = candle.Time.ToDateTime();

                    OHLC price = new OHLC(
                        open: openPriceCandle,
                        high: highPriceCandle,
                        low: lowPriceCandle,
                        close: closePriceCandle,
                        timeStart: candleTime,
                        timeSpan: TimeSpan.FromMinutes(_currentCandleHistoricalIntervalIndex));

                    prices.Add(price);
                }

                OHLC[] pricesArray = prices.ToArray();

                return pricesArray;
            }
            catch (Exception)
            {
                // Обработка ошибок
                return null; // Или выбросить исключение, в зависимости от вашей логики
            }
        }


        //  Метод получения свойства из CandleInterval по индексу, который получаем при скролле, чтобы сформировать таймфрейм свечи
        public static CandleInterval GetCandleIntervalByIndex(int index)
        {
            switch (index)
            {
                case 1:
                    return CandleInterval._1Min;
                case 2:
                    return CandleInterval._2Min;
                case 3:
                    return CandleInterval._3Min;
                case 4:
                    return CandleInterval._5Min;
                case 5:
                    return CandleInterval._10Min;
                case 6:
                    return CandleInterval._15Min;
                default:
                    // Если индекс больше или равен длине CandleInterval, вернуть _15Min
                    if (index >= System.Enum.GetValues(typeof(CandleInterval)).Length)
                    {
                        return CandleInterval._15Min;
                    }
                    // Если индекс меньше 0, вернуть _1Min
                    else if (index <= 0)
                    {
                        return CandleInterval._1Min;
                    }
                    // Иначе вернуть соответствующий интервал
                    else
                    {
                        return (CandleInterval)index;
                    }
            }
        }

    }
}
