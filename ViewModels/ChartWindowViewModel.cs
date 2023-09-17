using ScottPlot;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Tinkoff.InvestApi;
using Tinkoff.InvestApi.V1;
using TinkoffTradeSimulator.ApiServices;
using TinkoffTradeSimulator.ApiServices.Tinkoff;
using TinkoffTradeSimulator.Infrastacture.Commands;
using TinkoffTradeSimulator.Models;
using TinkoffTradeSimulator.Utils;
using TinkoffTradeSimulator.ViewModels.Base;
using TinkoffTradeSimulator.Views.Windows;

namespace TinkoffTradeSimulator.ViewModels
{
    class ChartWindowViewModel : BaseViewModel
    {

        #region Приватные свойства
        private InvestApiClient? _client = null;
        private WpfPlot? _wpfPlot = null;
        private string _title = string.Empty;

        // Приватное свойство для определения временно интервала свечей
        private int _selectedHistoricalTimeCandleIndex = 10;

        // Приватное свойство для хранения данных свечей
        private ObservableCollection<OHLC>? _candlestickData = null;

        private ChartToolTipManager? _сhartToolTipManager = null;

        private ToolTip? _toolTipInfo = null;


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
        public int SelectedHistoricalTimeCandleIndex
        {
            get => (int)_selectedHistoricalTimeCandleIndex;
            set => Set(ref _selectedHistoricalTimeCandleIndex, value);
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
        public ICommand? OpenCandleIntervalWindowCommand { get; } = null;

        private bool CanOpenCandleIntervalWindowCommandExecute(object p) => true;

        private void OnOpenCandleIntervalWindowCommandExecuted(object sender)
        {
            // Открываю окно с выбором таймфрема для свечи
            OpenCandleIntervalWindow();
        }
        #endregion

        // Пустой (необходимый) конструктор
        public ChartWindowViewModel()
        {
            #region Инициализация команд
            OpenCandleIntervalWindowCommand = new LambdaCommand(OnOpenCandleIntervalWindowCommandExecuted, CanOpenCandleIntervalWindowCommandExecute);
            #endregion
        }

        // Коснтурктор с перегрузами
        public ChartWindowViewModel(WpfPlot plot, string ticker)
        {
            #region Инициализация команд
            OpenCandleIntervalWindowCommand = new LambdaCommand(OnOpenCandleIntervalWindowCommandExecuted, CanOpenCandleIntervalWindowCommandExecute);
            #endregion

            // Делаю доступным в этой области видимости полученный объект из конструктора
            _wpfPlot = plot;
            // Так можно изменить стили для окна
            //_wpfPlot.plt.Style(
            //    figureBackground: Color.DarkBlue,
            //    dataBackground: Color.DarkGoldenrod
            //    );

            List<CandleTimeFrameButton> candleTimeFrames = new List<CandleTimeFrameButton>
            {
                new CandleTimeFrameButton { Name = "1 минута", Time = TimeSpan.FromMinutes(1) },
                new CandleTimeFrameButton { Name = "5 минут", Time = TimeSpan.FromMinutes(5) },
                new CandleTimeFrameButton { Name = "15 минут", Time = TimeSpan.FromMinutes(15) },
                // Добавьте другие таймфреймы по аналогии
            };


            ToolTipInfo = new ToolTip();

            string tickerName = ticker;

            // Загрузка каких-нибудь асинхронных данных
            LoadAsyncData();

            // Строю график и показываю по тикеру
            GetAndSetCandlesIntoView(tickerName, SelectedHistoricalTimeCandleIndex);

            //SetDataToView();
        }

        #region Методы

        private async void LoadAsyncData()
        {
            // Создаю клиента Тинькофф 
            _client = await TinkoffClient.CreateAsync();
        }

        // Метод получения свечей
        public async void GetAndSetCandlesIntoView(string ticker, int candleHistoricalIntervalIndex)
        {
            Title = ticker;

            try
            {
                TinkoffTradingPrices tinkoff = new TinkoffTradingPrices(_client);
                Share instrument = await tinkoff.GetShareByTicker(ticker);

                // Получение интервала в минутах
                //int selectedIntervalInMinutes = GetCandleIntervalByIndex(candleHistoricalIntervalIndex);

                // Опередляю за какой временной интервал получать свечи
                TimeSpan timeFrame = TimeSpan.FromMinutes(1000);

                List<HistoricCandle> candles = await tinkoff.GetCandles(instrument, timeFrame, 1);

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
                        timeSpan: TimeSpan.FromMinutes(1));

                    prices.Add(price);
                }

                OHLC[] pricesArray = prices.ToArray();

                // Очищаем текущий график перед добавлением новых свечей
                _wpfPlot?.Plot.Clear();

                _wpfPlot?.Plot.AddCandlesticks(pricesArray);


                // Обновляю информацию в TollTip
                //UpdateToolTipInfo( selectedIntervalInMinutes);

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

        // Открываю окно с выбором таймфрема
        private static void OpenCandleIntervalWindow()
        {
            CandleIntervalWindow candleIntervalWindow = new();

            // Получаем активное окно (ChartWindow)
            Window activeWindow = Application.Current.Windows.OfType<ChartWindow>().SingleOrDefault(x => x.IsActive);

            if (activeWindow != null)
            {
                candleIntervalWindow.Owner = activeWindow;
                candleIntervalWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }

            candleIntervalWindow.Show();
        }

        #region Выбор таймфрейма свечи       

        //// Метод увеличения таймфрейма свечи
        public void IncreaseCandleInterval()
        {
            // Максимально допустимый индекс для выбора таймфрейма
            int maxIndex = 100;

            SelectedHistoricalTimeCandleIndex++;
            if (SelectedHistoricalTimeCandleIndex > maxIndex) SelectedHistoricalTimeCandleIndex = maxIndex;

            GetAndSetCandlesIntoView(Title, SelectedHistoricalTimeCandleIndex);
        }

        //// Метод уменьшения таймфрейма свечи
        public void DecreaseCandleInterval()
        {
            // Минимально допустимый индекс для выбора таймфрейма
            int minxIndex = 1;

            SelectedHistoricalTimeCandleIndex--;
            if (SelectedHistoricalTimeCandleIndex < minxIndex) SelectedHistoricalTimeCandleIndex = minxIndex;

            GetAndSetCandlesIntoView(Title, SelectedHistoricalTimeCandleIndex);
        }

        //private void UpdateToolTipInfo(int timeFrame)
        //{
        //    ToolTipInfo.HorizontalOffset = 20;
        //    ToolTipInfo.VerticalOffset = 20;

        //    ToolTipInfo.Content = $"Выбранный таймфрейм свечи: {timeFrame} минут";
        //}
        #endregion

        #endregion
    }
}
