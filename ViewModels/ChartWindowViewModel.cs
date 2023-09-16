﻿using ScottPlot;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Tinkoff.InvestApi;
using Tinkoff.InvestApi.V1;
using TinkoffTradeSimulator.ApiServices;
using TinkoffTradeSimulator.ApiServices.Tinkoff;
using TinkoffTradeSimulator.ViewModels.Base;

namespace TinkoffTradeSimulator.ViewModels
{
    class ChartWindowViewModel : BaseViewModel
    {

        #region Приватные свойства
        private InvestApiClient? _client = null;
        private WpfPlot? _wpfPlot = null;
        private string _title;

        // Приватное свойство для определения индекса по которому будет выбран CandleInterval
        private int _selectedCandleIndex = 1;


        // Приватное свойство для хранения данных свечей
        private ObservableCollection<OHLC> _candlestickData;
        #endregion

        #region Публичные свойства
        public string Title
        {
            get => _title;
            set => Set(ref _title, value);
        }

        // Публичное свойство для хранения данных свечей
        public ObservableCollection<OHLC> CandlestickData
        {
            get => _candlestickData;
            set => Set(ref _candlestickData, value);
        }


        // Публичное свойство для определения индекса по которому будет выбран CandleInterval
        public int SelectedCandleIndex
        {
            get => (int)_selectedCandleIndex;
            set => Set(ref _selectedCandleIndex, value);
        }
        #endregion

        #region Команды

        #endregion

        // Пустой (необходимый) конструктор
        public ChartWindowViewModel() { }

        // Коснтурктор с перегрузами
        public ChartWindowViewModel(WpfPlot plot, string ticker)
        {
            // Делаю доступным в этой области видимости полученный объект из конструктора
            _wpfPlot = plot;

            string tickerName = ticker;

            // Загрузка каких-нибудь асинхронных данных
            LoadAsyncData();

            // Строю график и показываю по тикеру
            GetAndSetCandlesIntoView(tickerName, SelectedCandleIndex);

            //SetDataToView();
        }


        #region Методы

        // Тестовые данны для построения свечей
        //public void TestingDataA()
        //{
        //    // Each candle is represented by a single OHLC object.
        //    OHLC price = new(
        //        open: 100,
        //        high: 120,
        //        low: 80,
        //        close: 105,
        //        timeStart: new DateTime(1985, 09, 24),
        //        timeSpan: TimeSpan.FromDays(1));

        //    // Users could be build their own array of OHLCs, or lean on 
        //    // the sample data generator to simulate price data over time.
        //    OHLC[] prices = DataGen.RandomStockPrices(new Random(0), 60);

        //    _wpfPlot.Plot.AddColorbar();
        //    _wpfPlot.Plot.AddBubblePlot();
        //    // Add a financial chart to the plot using an array of OHLC objects
        //    _wpfPlot.Plot.AddCandlesticks(prices);

        //    WpfPlot wpfPlot = new WpfPlot();

        //    _wpfPlot.Refresh();
        //}

        // Метод для асинхронной загрузки данных(любых данных) обобщающий метод
        private async void LoadAsyncData()
        {
            // Создаю клиента Тинькофф 
            _client = await TinkoffClient.CreateAsync();
        }

        public async void GetAndSetCandlesIntoView(string ticker, int candleIntervalIndex)
        {

            // На вский случай обновляю заголовок окна чтобы он был согласован
            Title = ticker;
            try
            {
                TinkoffTradingPrices tinkoff = new TinkoffTradingPrices(_client);
                //tinkoff.GetCandleIntervalByIndex(candleIntervalIndex);

                // TODO надо разобраться почему не передаётся тикер. значение обнуляется

                Share instrument = await tinkoff.GetShareByTicker(ticker);

                TimeSpan timeFrame = TimeSpan.FromMinutes(1000);

                // получаем список свечей за определенный промежуток времени по Share
                List<HistoricCandle> candles = await tinkoff.GetCandles(instrument, timeFrame, candleIntervalIndex);

                // создаем список свечей OHLC
                List<OHLC> prices = new List<OHLC>();

                foreach (var candle in candles)
                {
                    // Приводим данные к типу OHLC
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
                        timeSpan: TimeSpan.FromMinutes(candleIntervalIndex));

                    prices.Add(price);
                }

                // Преобразуем список в массив OHLC[]
                OHLC[] pricesArray = prices.ToArray();

                // Добавляем сформированные данные к графику
                _wpfPlot?.Plot.AddCandlesticks(pricesArray);

                // Обновляем график
                _wpfPlot?.Refresh();
            }
            catch (Exception)
            {
                // Обрабатываем ошибки
                // Можно выводить сообщение об ошибке или выполнять другие действия
            }
        }


        #region Выбор таймфрейма свечи       

        // Метод увеличения таймфрейма свечи
        public void IncreaseCandleInterval()
        {
            SelectedCandleIndex++;
            GetAndSetCandlesIntoView(Title, SelectedCandleIndex);
        }

        // Метод уменьшения таймфрейма свечи
        public void DecreaseCandleInterval()
        {          
            SelectedCandleIndex--;
            GetAndSetCandlesIntoView(Title, SelectedCandleIndex);
        }

        #endregion

        #endregion
    }
}
