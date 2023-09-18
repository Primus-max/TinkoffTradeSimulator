using ScottPlot;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
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
            // TODO разобраться какая инициализация лишняя
            #region Инициализация команд
            OpenCandleIntervalWindowCommand = new LambdaCommand(OnOpenCandleIntervalWindowCommandExecuted, CanOpenCandleIntervalWindowCommandExecute);
            #endregion
        }

        // Коснтурктор с перегрузами
        public ChartWindowViewModel(WpfPlot plot, string ticker)
        {
            Title = ticker;

            // TODO разобраться какая инициализация лишняя
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

            ToolTipInfo = new ToolTip();

            string tickerName = ticker;

            // Загрузка каких-нибудь асинхронных данных
            LoadAsyncData();

            // Строю график и показываю по тикеру
           

            //SetDataToView();
        }

        #region Методы
        private async void LoadAsyncData()
        {
            // Создаю клиента Тинькофф 
            _client = await TinkoffClient.CreateAsync();

            // Получаю данные по тикерам (передаю именованные параметры)
           await GetAndSetCandlesIntoView(ticker: Title, candleHistoricalIntervalIndex: SelectedHistoricalTimeCandleIndex);
        }

        // Метод получения свечей
        // Поля или свойства для хранения текущих значений
        private string _currentTicker = "DefaultTicker";
        private int _currentCandleHistoricalIntervalIndex = 0;

        public async Task GetAndSetCandlesIntoView(string ticker = null, int? candleHistoricalIntervalIndex = null, CandleInterval? candleInterval = null)
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

            Title = _currentTicker;

            try
            {
                TinkoffTradingPrices tinkoff = new TinkoffTradingPrices(_client);
                Share instrument = await tinkoff.GetShareByTicker(_currentTicker);

                // Определяем временной интервал для запроса свечей
                TimeSpan timeFrame = TimeSpan.FromMinutes(_currentCandleHistoricalIntervalIndex);

                // Определение CandleInterval на основе параметра или значения по умолчанию
                CandleInterval interval = candleInterval ?? CandleInterval._1Min;

                List<HistoricCandle> candles = await tinkoff.GetCandles(instrument, timeFrame, interval);

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

                // Очищаем текущий график перед добавлением новых свечей
                _wpfPlot?.Plot.Clear();

                _wpfPlot?.Plot.AddCandlesticks(pricesArray);

                // Обновляем информацию в ToolTip
                // UpdateToolTipInfo(selectedIntervalInMinutes);

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

        #region Выбор исторического интервала для свечей       

        //// Метод увеличения таймфрейма свечи
        public void IncreaseCandleHistorical()
        {
            // Максимально допустимый индекс для выбора таймфрейма
            int maxIndex = 100;

            SelectedHistoricalTimeCandleIndex++;
            if (SelectedHistoricalTimeCandleIndex > maxIndex) SelectedHistoricalTimeCandleIndex = maxIndex;

            GetAndSetCandlesIntoView(Title, SelectedHistoricalTimeCandleIndex);
        }

        // Метод уменьшения таймфрейма свечи
        public void DecreaseCandleIHistorical()
        {
            // Минимально допустимый индекс для выбора таймфрейма
            int minxIndex = 1;

            SelectedHistoricalTimeCandleIndex--;
            if (SelectedHistoricalTimeCandleIndex < minxIndex) SelectedHistoricalTimeCandleIndex = minxIndex;

            GetAndSetCandlesIntoView(Title, SelectedHistoricalTimeCandleIndex);
        }       
        #endregion

        #endregion
    }
}
