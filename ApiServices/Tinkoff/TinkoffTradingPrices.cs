using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Tinkoff.InvestApi;
using Tinkoff.InvestApi.V1;
using TinkoffTradeSimulator.Models;

namespace TinkoffTradeSimulator.ApiServices.Tinkoff
{
     public class TinkoffTradingPrices
    {
        private static InvestApiClient? _client = null;
        private static string _currentTicker = string.Empty;
        private static int _currentCandleHistoricalIntervalIndex = 10;
        private static CandleInterval _currentCandleInterval = CandleInterval._1Min;

        public TinkoffTradingPrices(InvestApiClient client)
        {
            _client = client;
        }


        // Получаем Share по тикеру
        public static async Task<Share> GetShareByTicker(string ticker)
        {
            Share share = new();
            try
            {
                // Получаю инструмент 
                SharesResponse sharesResponse = await _client?.Instruments.SharesAsync();
                share = sharesResponse?.Instruments?.FirstOrDefault(x => x.Ticker == ticker) ?? new Share();
            }
            catch (Exception)
            {
                //MessageBox.Show($"Не удалось получить инструмент. Причина: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return share;
        }

        // Получаю исторические свечи по Share и временному промежутку
        public static async Task<List<HistoricCandle>> GetCandles(Share instrument, TimeSpan timeFrame, CandleInterval candlInterval)
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
                Interval = candlInterval
            };

            try
            {
                // Отправляю запрос и получаю ответ по свечам
                var response = await _client?.MarketData.GetCandlesAsync(request);

                List<HistoricCandle>? candles = response.Candles?.ToList();

                return candles;
            }
            catch (Exception ex)
            {
                // Обработка ошибки при получении свечей
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);

                return new List<HistoricCandle>();
            }

        }

        // Метод обновления и получения данных по свечам, учитывая имя тикера, таймфрем, временной интервал (именовыные параметры)
        public static async Task<List<CandlestickData>> GetCandlesData(string ticker = null!,  int? candleHistoricalIntervalIndex = null,  CandleInterval? candleInterval = null)
        {
            if (_client == null) return new List<CandlestickData>();

            // Обновляем текущие значения, если параметры были переданы
            if (ticker != null)
            {
                _currentTicker = ticker;
            }

            if (candleHistoricalIntervalIndex != null)
            {
                _currentCandleHistoricalIntervalIndex = (int)candleHistoricalIntervalIndex;
            }

            if(candleInterval != null)
            {
                _currentCandleInterval = (CandleInterval)candleInterval;
            }

            try
            {
                // Метод получения информации по тикеру
                Share instrument = await GetShareByTicker(_currentTicker);

                int additionalTimeForCandles = CalcadditionalTimeForCandles(_currentCandleInterval); 
                int currentCandleHistoricalIntervalIndex = 100;
                int totalMinutes = additionalTimeForCandles + currentCandleHistoricalIntervalIndex;

                TimeSpan timeFrame = TimeSpan.FromMinutes(totalMinutes);
                                
                // Определение CandleInterval на основе параметра или значения по умолчанию
                //CandleInterval interval = candleInterval ?? CandleInterval._1Min;

                List<HistoricCandle> candles = await GetAtLeast100Candles(instrument, _currentCandleInterval);

                List<CandlestickData> candlestickData = new List<CandlestickData>();

                foreach (var candle in candles)
                {
                    // Преобразовываем данные в TinkoffTradeSimulator.Models.CandlestickData
                    double openPriceCandle = Convert.ToDouble(candle.Open);
                    double highPriceCandle = Convert.ToDouble(candle.High);
                    double lowPriceCandle = Convert.ToDouble(candle.Low);
                    double closePriceCandle = Convert.ToDouble(candle.Close);

                    // Преобразуем Timestamp в DateTime
                    DateTime candleTime = candle.Time.ToDateTime();

                    var candlestick = new CandlestickData
                    {
                        Date = candleTime,
                        High = highPriceCandle,
                        Low = lowPriceCandle,
                        Open = openPriceCandle,
                        Close = closePriceCandle
                    };

                    candlestickData.Add(candlestick);
                }

                return candlestickData;
            }
            catch (Exception)
            {
                // Обработка ошибок
                return new List<CandlestickData>();
            }
        }

        public static async Task<List<HistoricCandle>> GetAtLeast100Candles(Share instrument, CandleInterval candleInterval)
        {
            List<HistoricCandle> allCandles = new List<HistoricCandle>();
            int limit = CalcadditionalTimeForCandles(candleInterval);

            DateTimeOffset now = DateTimeOffset.Now;
            DateTimeOffset endDate = now; // Начинаем с текущей даты

            while (allCandles.Count < 100)
            {
                DateTimeOffset startDate = endDate.AddMinutes(-limit);

                Timestamp startDateTimestamp = Timestamp.FromDateTimeOffset(startDate);
                Timestamp endDateTimestamp = Timestamp.FromDateTimeOffset(endDate);

                var request = new GetCandlesRequest()
                {
                    InstrumentId = instrument.Uid,
                    From = startDateTimestamp,
                    To = endDateTimestamp,
                    Interval = candleInterval
                };

                try
                {
                    var response = await _client?.MarketData.GetCandlesAsync(request);

                    List<HistoricCandle>? candles = response.Candles?.ToList();

                    if (candles != null && candles.Any())
                    {
                        allCandles.InsertRange(0, candles); // Вставляем свечи в начало списка
                    }
                    else
                    {
                        // Если не удалось получить свечи, прерываем цикл
                        break;
                    }

                    // Устанавливаем конечную дату для следующего запроса
                    endDate = startDate;
                }
                catch (Exception ex)
                {
                    // Обработка ошибки при получении свечей
                    //MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    break;
                }
            }

            // Если свечей больше 100, возвращаем только последние 100
            if (allCandles.Count > 100)
            {
                return allCandles.TakeLast(100).ToList();
            }
            else
            {
                return allCandles;
            }
        }


        //  Метод добавления к общему времени получения свечей дополнительго времени (брать с запасом)
        public static int  CalcadditionalTimeForCandles(CandleInterval interaval)
        {
            switch (interaval)
            {
                case CandleInterval._1Min:
                case CandleInterval._2Min:
                case CandleInterval._3Min:
                case CandleInterval._5Min:
                case CandleInterval._10Min:
                case CandleInterval._15Min:
                    return (24 * 60 - _currentCandleHistoricalIntervalIndex);
                case CandleInterval._30Min:
                    return (2 * 24 * 60 - _currentCandleHistoricalIntervalIndex);
                case CandleInterval.Hour:
                    return (7 * 24 * 60 - _currentCandleHistoricalIntervalIndex);
                case CandleInterval._2Hour:
                    return (29 * 24 * 60 - _currentCandleHistoricalIntervalIndex);
                case CandleInterval._4Hour:
                    return (29 * 24 * 60 - _currentCandleHistoricalIntervalIndex);
                case CandleInterval.Day:
                    return (360 * 24 * 60 - _currentCandleHistoricalIntervalIndex);
                default:
                    return (24 * 60);
            }
        }
    }
}
