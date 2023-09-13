using ScottPlot;
using System;
using System.CodeDom;
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

        public ChartWindowViewModel()
        {
                
        }
        public ChartWindowViewModel(WpfPlot  plot)
        {
            _wpfPlot = plot;

            TestingData();
            // Заголовок окна
            CandlestickData = new ObservableCollection<OHLC>();

        //    // Создаем тестовые данные для свечей
        //    OHLC[] testData = new OHLC[]
        //    {
        //new OHLC(100, 120, 80, 105, new DateTime(1985, 09, 24), TimeSpan.FromDays(1)),
        //        // Добавьте еще свои тестовые данные здесь...
        //    };

        //    // Заполняем коллекцию свечей
        //    foreach (var dataPoint in testData)
        //    {
        //        CandlestickData.Add(dataPoint);
        //    }

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
            List<HistoricCandle> customCandle = await tinkoff.GetCandles(instrument, timeFrame);



        }

        #endregion
    }
}
