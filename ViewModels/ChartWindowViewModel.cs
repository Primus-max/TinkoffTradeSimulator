using ScottPlot;
using ScottPlot.Plottable;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using Tinkoff.InvestApi;
using Tinkoff.InvestApi.V1;
using TinkoffTradeSimulator.ApiServices;
using TinkoffTradeSimulator.ApiServices.Tinkoff;
using TinkoffTradeSimulator.Utils;
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

        private ChartToolTipManager _сhartToolTipManager;

        private ToolTip _toolTipInfo;
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

        public ChartToolTipManager ChartToolTipManager
        {
            get => _сhartToolTipManager;
            set => Set(ref _сhartToolTipManager, value);
        }

        public ToolTip ToolTipInfo
        {
            get => _toolTipInfo;
            set => Set(ref _toolTipInfo, value);
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
            // Так можно изменить стили для окна
            //_wpfPlot.plt.Style(
            //    figureBackground: Color.DarkBlue,
            //    dataBackground: Color.DarkGoldenrod
            //    );

            ToolTipInfo = new ToolTip();

            string tickerName = ticker;

            // Загрузка каких-нибудь асинхронных данных
            LoadAsyncData();

            // Строю график и показываю по тикеру
            GetAndSetCandlesIntoView(tickerName, SelectedCandleIndex);

            //SetDataToView();
        }


        #region Методы

        private async void LoadAsyncData()
        {
            // Создаю клиента Тинькофф 
            _client = await TinkoffClient.CreateAsync();
        }

        // Метод получения свечей
        public async void GetAndSetCandlesIntoView(string ticker, int candleIntervalIndex)
        {
            Title = ticker;

            try
            {
                TinkoffTradingPrices tinkoff = new TinkoffTradingPrices(_client);
                Share instrument = await tinkoff.GetShareByTicker(ticker);

                // Получение интервала в минутах
                int selectedIntervalInMinutes = GetCandleIntervalByIndex(candleIntervalIndex);

                TimeSpan timeFrame = TimeSpan.FromMinutes(1000);

                List<HistoricCandle> candles = await tinkoff.GetCandles(instrument, timeFrame, candleIntervalIndex);

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
                        timeSpan: TimeSpan.FromMinutes(selectedIntervalInMinutes));

                    prices.Add(price);
                }

                OHLC[] pricesArray = prices.ToArray();

                // Очищаем текущий график перед добавлением новых свечей
                _wpfPlot?.Plot.Clear();

                _wpfPlot?.Plot.AddCandlesticks(pricesArray);


                // Обновляю информацию в TollTip
                UpdateToolTipInfo( selectedIntervalInMinutes);

                _wpfPlot?.Refresh();
            }
            catch (Exception)
            {
                // Обработка ошибок
            }
        }

        // Получаю колличество минут для таймфрема по индексу который получаем при скролее
        private static int GetCandleIntervalByIndex(int index)
        {
            // Перечень интервалов свечей в минутах
            int[] candleIntervalsInMinutes = new int[] { 1, 2, 3, 5, 10, 15 };

            // Убедимся, что индекс в допустимом диапазоне
            if (index < 1)
            {
                index = 1;
            }
            else if (index > candleIntervalsInMinutes.Length)
            {
                index = candleIntervalsInMinutes.Length;
            }

            // Получим интервал в минутах
            int candleInterval = candleIntervalsInMinutes[index - 1];

            return candleInterval;
        }


        #region Выбор таймфрейма свечи       

        // Метод увеличения таймфрейма свечи
        public void IncreaseCandleInterval(ToolTip toolTip)
        {
            // Максимально допустимый индекс для выбора таймфрейма
            int maxIndex = 6;

            SelectedCandleIndex++;
            if (SelectedCandleIndex > maxIndex) SelectedCandleIndex = maxIndex;
            GetAndSetCandlesIntoView(Title, SelectedCandleIndex);

            ToolTipInfo = toolTip;
        }

        // Метод уменьшения таймфрейма свечи
        public void DecreaseCandleInterval(ToolTip toolTip)
        {
            // Минимально допустимый индекс для выбора таймфрейма
            int minxIndex = 1;

            SelectedCandleIndex--;
            if (SelectedCandleIndex < minxIndex) SelectedCandleIndex = minxIndex;
            GetAndSetCandlesIntoView(Title, SelectedCandleIndex);

            ToolTipInfo = toolTip;

        }

        private void UpdateToolTipInfo(int timeFrame)
        {
            ToolTipInfo.HorizontalOffset = 20;
            ToolTipInfo.VerticalOffset = 20;

            ToolTipInfo.Content = $"Выбранный таймфрейм свечи: {timeFrame} минут";
        }
        #endregion

        #endregion
    }
}
