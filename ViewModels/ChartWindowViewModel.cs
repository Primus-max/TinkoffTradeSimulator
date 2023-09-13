using ScottPlot;
using ScottPlot.Drawing.Colormaps;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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

        #endregion

        #region Команды

        #endregion

        public ChartWindowViewModel()
        {

        }
        public ChartWindowViewModel(WpfPlot plot)
        {
            // Делаю доступным в этой области видимости полученный объект из конструктора
            _wpfPlot = plot;
            
            // Загрузка каких-нибудь асинхронных данных
            LoadAsyncData();
        }

        #region Методы

        public void TestingData()
        {
            // Each candle is represented by a single OHLC object.
            OHLC price = new(
                open: 100,
                high: 120,
                low: 80,
                close: 105,
                timeStart: new DateTime(1985, 09, 24),
                timeSpan: TimeSpan.FromDays(1));

            // Users could be build their own array of OHLCs, or lean on 
            // the sample data generator to simulate price data over time.
            OHLC[] prices = DataGen.RandomStockPrices(new Random(0), 60);

            _wpfPlot.Plot.AddColorbar();
            _wpfPlot.Plot.AddBubblePlot();
            // Add a financial chart to the plot using an array of OHLC objects
            _wpfPlot.Plot.AddCandlesticks(prices);

            WpfPlot wpfPlot = new WpfPlot();

            _wpfPlot.Refresh();
        }

        // Метод для асинхронной загрузки данных(любых данных) обобщающий метод
        private async void LoadAsyncData()
        {
            // Создаю клиента Тинькофф 
            _client = await TinkoffClient.CreateAsync();
        }

        public async void SetDataToView()
        {
            TinkoffTradingPrices tinkoff = new TinkoffTradingPrices(_client);
            // получаю инструмент (по имени тикера)

            Share instrument = await tinkoff.GetShareByTicker(Title);

            TimeSpan timeFrame = TimeSpan.FromMinutes(1000);

            // Получаю список свечей за определённый промежуток времени по Share
            List<HistoricCandle> candels = await tinkoff.GetCandles(instrument, timeFrame);


            // Привожу полученные свечи к типу данных для OHLC
            OHLC[] prices = null;

            // Прохожу по свечам полученным от Тинькофф и формирую для отображении во View
            foreach (var candle in candels)
            {
                // Привожу к типу данных от Тинькофф к объекту OHLC для WpfPlot
                double openPriceCandle = Convert.ToDouble(candle.Open);
                double highPriceCandle = Convert.ToDouble(candle.High);
                double lowPriceCandle = Convert.ToDouble(candle.Low);
                double closePriceCandle = Convert.ToDouble(candle.Close);

                
                OHLC price = new(
                    open: openPriceCandle,
                    high: highPriceCandle,
                    low: lowPriceCandle,
                    close: closePriceCandle,
                    timeStart: new DateTime(1985, 09, 24),
                    timeSpan: TimeSpan.FromDays(1));

                prices = new OHLC[] { price };
            }
                        
            // Добавляю сформированные данные от Тинькофф в объект для отображения в виде свечей
            _wpfPlot?.Plot.AddCandlesticks(prices);

            // Обязательно надо обновлять объект для кореектного отображения данных
            _wpfPlot.Refresh();

        }

        #endregion
    }
}
