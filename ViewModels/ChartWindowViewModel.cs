using DromAutoTrader.Data;
using DromAutoTrader.Views;
using Microsoft.EntityFrameworkCore;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Tinkoff.InvestApi;
using Tinkoff.InvestApi.V1;
using TinkoffTradeSimulator.ApiServices;
using TinkoffTradeSimulator.ApiServices.Tinkoff;
using TinkoffTradeSimulator.Infrastacture.Commands;
using TinkoffTradeSimulator.Models;
using TinkoffTradeSimulator.Services;
using TinkoffTradeSimulator.ViewModels.Base;
using TinkoffTradeSimulator.Views.Windows;

namespace TinkoffTradeSimulator.ViewModels
{
    internal class ChartWindowViewModel : BaseViewModel
    {

        #region Приватные свойства
        private InvestApiClient? _client = null;
        private string _title = string.Empty;
        private AppContext _db = null!;
        private PlotView _plotModel = null!;
        private int _selectedHistoricalTimeCandleIndex = 10;
        private CandleTimeFrameButton _selectedTimeFrame = new CandleTimeFrameButton
        {
            Name = CandleInterval._1Min.ToString(), // При инициализации значение по дефолту
            Time = _intervalMapping[CandleInterval._1Min.ToString()] // При инициализации значение по дефолту
        };
        private int _volumeTradingTicker = 1;
        private ObservableCollection<HistoricalTradeRecordInfo> _tradeHistoricalInfoList = null!;
        private ObservableCollection<TradeRecordInfo> _tradeCurrentInfoList = null!;
        private TickerInfo _stockInfo = null!;
        private string _ticker = string.Empty;
        private ObservableCollection<CandlestickData> _candlestickData = null!;
        private TickerInfo _tickerInfo = null!;
        private static Dictionary<string, TimeSpan> _intervalMapping = new Dictionary<string, TimeSpan>
        {
            { "_1Min", TimeSpan.FromMinutes(1) },
            { "_2Min", TimeSpan.FromMinutes(2) },
            { "_3Min", TimeSpan.FromMinutes(3) },
            { "_5Min", TimeSpan.FromMinutes(5) },
            { "_10Min", TimeSpan.FromMinutes(10) },
            { "_15Min", TimeSpan.FromMinutes(15) },
            { "_30Min", TimeSpan.FromMinutes(30) },
            { "Hour", TimeSpan.FromHours(1) },
            { "_2Hour", TimeSpan.FromHours(2) },
            { "_4Hour", TimeSpan.FromHours(4) },
            { "Day", TimeSpan.FromDays(1) },
        };
        private List<CandlestickData> _localStorageLastTickers = null!;
        #endregion

        #region Публичные свойства
        public string Title
        {
            get => _title;
            set => Set(ref _title, value);
        }
        public ObservableCollection<CandlestickData> CandlestickData
        {
            get => _candlestickData;
            set => Set(ref _candlestickData, value);

        }
        public int SelectedHistoricalTimeCandleIndex
        {
            get => (int)_selectedHistoricalTimeCandleIndex;
            set => Set(ref _selectedHistoricalTimeCandleIndex, value);
        }
        public CandleTimeFrameButton SelectedTimeFrame
        {
            get => _selectedTimeFrame;
            set => Set(ref _selectedTimeFrame, value);
        }
        public int VolumeTradingTicker
        {
            get => _volumeTradingTicker;
            set => Set(ref _volumeTradingTicker, value);
        }
        public TickerInfo StockInfo
        {
            get => _stockInfo;
            set => Set(ref _stockInfo, value);
        }

        public PlotView PlotModel
        {
            get => _plotModel;
            set => Set(ref _plotModel, value);
        }
        public TickerInfo TickerInfo
        {
            get => _tickerInfo;
            set => Set(ref _tickerInfo, value);
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

        #region Покупаю тикер
        public ICommand? BuyTickerCommand { get; } = null;

        private bool CanBuyTickerCommandExecute(object p) => true;

        private void OnBuyTickerCommandExecuted(object sender)
        {
            TickerInfo = LocatorService.Current.TickerInfo;

            if (TickerInfo == null)
            {
                MessageBox.Show("Данные о тикере не были загружены. Убедитесь, что цена тикера отображается и попробуйте еще раз", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            double tickerPrice = Convert.ToDouble(TickerInfo.Price);

            // Совершаю сделку
            TradeManager.BuyTicker(Title, tickerPrice, VolumeTradingTicker);

            // Обновляю источники данных после совершения сделки
            UpdateTradingInfoAfterExecuteTrade();
        }

        #endregion

        #region Продаю тикер
        public ICommand? SellickerCommand { get; } = null;

        private bool CanSellickerCommandExecute(object p) => true;

        private void OnSellickerCommandExecuted(object sender)
        {
            TickerInfo = LocatorService.Current.TickerInfo;

            if (TickerInfo == null)
            {
                MessageBox.Show("Данные о тикере не были загружены. Убедитесь, что цена тикера отображается и попробуйте еще раз", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            double tickerPrice = Convert.ToDouble(TickerInfo.Price);

            // Совершаю сделку
            TradeManager.SellTicker(Title, tickerPrice, VolumeTradingTicker);

            // Обновляю источники данных после совершения сделки
            UpdateTradingInfoAfterExecuteTrade();
        }
        #endregion

        #endregion

        // Пустой (необходимый) конструктор
        public ChartWindowViewModel()
        {
            // TODO разобраться какая инициализация лишняя
            #region Инициализация команд
            OpenCandleIntervalWindowCommand = new LambdaCommand(OnOpenCandleIntervalWindowCommandExecuted, CanOpenCandleIntervalWindowCommandExecute);
            BuyTickerCommand = new LambdaCommand(OnBuyTickerCommandExecuted, CanBuyTickerCommandExecute);
            SellickerCommand = new LambdaCommand(OnSellickerCommandExecuted, CanSellickerCommandExecute);
            #endregion

            #region Инициализация базы данных
            InitializeDatabase();
            #endregion

            #region Инициализация источников данных
            _tradeHistoricalInfoList = new ObservableCollection<HistoricalTradeRecordInfo>();
            _tradeCurrentInfoList = new ObservableCollection<TradeRecordInfo>();
            #endregion

            #region Подписки на события
            // Подписываемся на событие выбранного таймфрейма CandleIntervalSelected
            EventAggregator.CandleIntervalSelected += OnCandleIntervalSelected;
            #endregion

            //StockInfo = new TickerInfo();
            PlotModel = new PlotView();
            // Ваша логика загрузки данных о свечах должна быть здесь
            CandlestickData = new ObservableCollection<CandlestickData>();
            //UpdateData(1);
            LoadAsyncData();
        }

        #region Методы
        // Метод установки или обновления свечей для отображения в графике
        public void SetAndUpdateCandlesChartWindow()
        {        
            // TODO логика получения нужных тикеров из хранилища
            List<CandlestickData> candlestickData = GetCandlestickData();

            // Создаю объект для отображения свечей
            var plotModel = CreateCandlestickPlotModel();

            // Очистите старые серии данных из PlotModel
            plotModel.Series.Clear();

            var candlestickSeries = new CandleStickSeries
            {
                Title = "Candlesticks",
                Background = OxyColor.FromAColor(128, OxyColor.Parse("#A5424B51")) ,
                 
                
                                                                                  
            };

            foreach (var candle in candlestickData)
            {
                // Устанавливаем формат ToolTip для каждой свечи
                candlestickSeries.TrackerFormatString = $"Date: {candle.Date:yyyy-MM-dd HH:mm}\nOpen: {candle.Open}\nHigh: {candle.High}\nLow: {candle.Low}\nClose: {candle.Close}";

                candlestickSeries.Items.Add(new HighLowItem(
                    DateTimeAxis.ToDouble(candle.Date),
                    candle.High,
                    candle.Low,
                    candle.Open,
                    candle.Close
                ));
            }

            // TODO Вынести в отдельный метод
            #region Формирование инфомрации о тикере для отображения

            try
            {
                // Определение актуальной цены (последняя свеча в списке)
                var lastCandle = candlestickData.LastOrDefault();
                string actualPrice = lastCandle?.Close.ToString("F2"); // Предполагая, что Close является ценой закрытия

                // Определение максимальной и минимальной цен
                var maxPrice = candlestickData.Max(candle => candle.High);
                var minPrice = candlestickData.Min(candle => candle.Low);


                // Создание объекта TickerInfo
                var tickerInfo = new TickerInfo
                {
                    TickerName = Title,
                    Price = actualPrice,
                    MaxPrice = maxPrice.ToString("F2"),
                    MinPrice = minPrice.ToString("F2")
                };

                EventAggregator.PublishUpdateTickerInfo(tickerInfo);
            }
            catch (Exception)
            {

            }

            #endregion

            // TODO Вынести в отдельный метод
            plotModel.Series.Add(candlestickSeries);
            PlotModel.Model = plotModel;

            // ОБновляю PlotModel, чтобы обновить график
            PlotModel.InvalidatePlot(true);
        }

        // Метод создания объекта модели отображения свечей
        private PlotModel CreateCandlestickPlotModel()
        {
            var plotModel = new PlotModel
            {
                Title = $"График свечей для {Title}",
                Culture = System.Globalization.CultureInfo.CurrentCulture
            };

            var horizontalAxis = new DateTimeAxis
            {
                Position = AxisPosition.Bottom,
                IsZoomEnabled = false,
                StringFormat = "HH:mm", // Формат времени
                MaximumPadding = 0.03,
                MinimumPadding = 0.03,
                MajorGridlineStyle = LineStyle.Dot,
                MinorGridlineStyle = LineStyle.Dot,
                MajorGridlineColor = OxyColor.FromRgb(44, 44, 44),
                TicklineColor = OxyColor.FromRgb(82, 82, 82)
            };

            plotModel.Axes.Add(horizontalAxis);

            plotModel.Axes.Add(new LinearAxis { IsPanEnabled = true, IsZoomEnabled = false }); // Горизонтальная ось
            //plotModel.Axes.Add(new LinearAxis { IsPanEnabled = true, IsZoomEnabled = false, Position = AxisPosition.Bottom }); // Вертикальная ось
            return plotModel;
        }

        // Метод получения выбранной кнопки для отображения имени
        private async void OnCandleIntervalSelected(CandleTimeFrameButton selectedButton)
        {
            if (selectedButton == null || _intervalMapping == null)
            {
                // Добавьте обработку null, если необходимо.
                return;
            }

            SelectedTimeFrame = selectedButton;
            string? intervalName = SelectedTimeFrame.Name;

            if (_intervalMapping.TryGetValue(intervalName, out TimeSpan time))
            {
                SelectedTimeFrame.Time = time;
                await GetLastCandlesForLocalSotarageAsync();
            }
            else
            {
                MessageBox.Show("Выбранный интервал не найден в словаре, попробуй еще раз", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Метод загрузки асинхронных данных для вызова из конструктора
        public async void LoadAsyncData()
        {
            // Создаю клиента Тинькофф 
            _client = await TinkoffClient.CreateAsync();

            TinkoffTradingPrices tinkoff = new TinkoffTradingPrices(_client);
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

        #region Выбор исторического интервала для свечей по скроллу       

        // Метод увеличения таймфрейма свечи
        public void IncreaseCandleHistorical()
        {
            int maxIndex = 100;

            SelectedHistoricalTimeCandleIndex += 1; // Увеличиваем на одну "единицу" времени
            if (SelectedHistoricalTimeCandleIndex > maxIndex) SelectedHistoricalTimeCandleIndex = maxIndex;

            SetAndUpdateCandlesChartWindow();
        }

        // Метод уменьшения таймфрейма свечи
        public void DecreaseCandleIHistorical()
        {
            // Минимально допустимый индекс для выбора таймфрейма
            int minxIndex = 10;

            SelectedHistoricalTimeCandleIndex -= 1;
            if (SelectedHistoricalTimeCandleIndex < minxIndex) SelectedHistoricalTimeCandleIndex = minxIndex;

            SetAndUpdateCandlesChartWindow();
        }

        #endregion

        // Получаю выбранное количество свечей из локального хранилища
        public List<CandlestickData> GetCandlestickData()
        {

            List<CandlestickData> candlestickData = new List<CandlestickData>();

            if (SelectedHistoricalTimeCandleIndex <= 0)
            {
                // Возвращаем пустой список, если запрошено 0 или отрицательное количество свечей
                return candlestickData;
            }

            int numberOfCandlesToRetrieve = CalculatedMinuteFromSelectedTimeFrame() > 100? 100 : CalculatedMinuteFromSelectedTimeFrame();

            // Ограничиваем количество свечей, чтобы не выйти за пределы массива
            if (numberOfCandlesToRetrieve > _localStorageLastTickers.Count)
            {
                numberOfCandlesToRetrieve = _localStorageLastTickers.Count;
            }

            // Получаем указанное количество свечей
            candlestickData = _localStorageLastTickers.TakeLast(numberOfCandlesToRetrieve).ToList();

            return candlestickData;
        }

        // Метод получения последних 100 свечей для локального хранения
        internal async Task GetLastCandlesForLocalSotarageAsync(string ticker = null!)
        {
            if (ticker == null) { }
            // По дефолту всегда получаю 100 единиц вермени, для локального хранения свечей
            int candleHistoricalIntervalByDefault = 100;

            // Получаю таймфрейм свечи (_1Min, _2Min, _15Min)
            string? selectedTimeframe = SelectedTimeFrame.Name;

            // Привожу к типу данных 
            CandleInterval candleInterval = (CandleInterval)Enum.Parse(typeof(CandleInterval), selectedTimeframe);

            // Заполняю локальное хранилище тикерами за последние 100
            _localStorageLastTickers = await TinkoffTradingPrices.GetCandlesData(ticker: ticker, candleHistoricalIntervalByDefault, candleInterval: candleInterval);

            // Вызываю обоновление или отображение списка
            SetAndUpdateCandlesChartWindow();
        }

        // Получаю минуты исходя из выбранного временого диапазона и выбранного таймфрема свечи
        private int CalculatedMinuteFromSelectedTimeFrame()
        {
            int totalMinute = 0;
            totalMinute = SelectedHistoricalTimeCandleIndex * (int)SelectedTimeFrame.Time.TotalMinutes;
            return totalMinute;
        }

        #region Методы по торговле (покупка/продажа)

        // Метод обновления источников данных после покупки или продажи тикеров
        private void UpdateTradingInfoAfterExecuteTrade()
        {
            // Опубликовываем событие для текущей коллекции
            EventAggregator.PublishTradingInfoChanged();
        }


        #endregion
        // Инициализация базы данных
        private void InitializeDatabase()
        {
            try
            {
                // Экземпляр базы данных
                _db = AppContextFactory.GetInstance();
                // загружаем данные о поставщиках из БД
                _db.HistoricalTradeRecordsInfo.Load();
                _db.TradeRecordsInfo.Load();
            }
            catch (Exception)
            {
                // TODO сделать запись логов
                //Console.WriteLine($"Не удалось инициализировать базу данных: {ex.Message}");
            }
        }

        #endregion
    }
}
