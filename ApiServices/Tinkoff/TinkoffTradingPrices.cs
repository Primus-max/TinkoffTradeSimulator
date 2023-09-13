using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tinkoff.InvestApi;
using Tinkoff.InvestApi.V1;

namespace TinkoffTradeSimulator.ApiServices.Tinkoff
{
    class TinkoffTradingPrices
    {
        private readonly InvestApiClient? _client = null;

        public TinkoffTradingPrices(InvestApiClient client)
        {
            _client = client;
        }

        // Получае Share по тикеру
        public async Task<Share> GetShareByTicker(string ticker)
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
        public async Task<List<HistoricCandle>> GetCandles(Share instrument, TimeSpan timeFrame)
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
                Interval = CandleInterval._1Min
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
    }
}
