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
using TinkoffTradeSimulator.Data;
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
        private int _selectedHistoricalTimeCandleIndex = 100;
        private CandleTimeFrameButton _selectedTimeFrame = new CandleTimeFrameButton { Name = CandleInterval._1Min.ToString() };
        private int _volumeTradingTicker = 1;
        private ObservableCollection<HistoricalTradeRecordInfo> _tradeHistoricalInfoList = null!;
        private ObservableCollection<TradeRecordInfo> _tradeCurrentInfoList = null!;
        private TickerInfo _stockInfo = null!;
        private string _ticker = string.Empty;
        private ObservableCollection<CandlestickData> _candlestickData = null!;
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
        #endregion

        #region Команды
        public ICommand? OpenCandleIntervalWindowCommand { get; } = null;

        private bool CanOpenCandleIntervalWindowCommandExecute(object p) => true;

        private void OnOpenCandleIntervalWindowCommandExecuted(object sender)
        {
            // Открываю окно с выбором таймфрема для свечи
            OpenCandleIntervalWindow();
        }

        public ICommand? BuyTickerCommand { get; } = null;

        private bool CanBuyTickerCommandExecute(object p) => true;

        private void OnBuyTickerCommandExecuted(object sender)
        {
            BuyTicker();
        }

        public ICommand? SellickerCommand { get; } = null;

        private bool CanSellickerCommandExecute(object p) => true;

        private void OnSellickerCommandExecuted(object sender)
        {
            SellTicker();
        }

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
            DbManager dbManager = new();
            _db = dbManager.InitializeDB();
            #endregion

            #region Инициализация источников данных
            _tradeHistoricalInfoList = new ObservableCollection<HistoricalTradeRecordInfo>();
            _tradeCurrentInfoList = new ObservableCollection<TradeRecordInfo>();
            StockInfo = new TickerInfo();
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
        public async Task SetAndUpdateCandlesChartWindow(string ticker = null!, int? candleHistoricalIntervalIndex = null, CandleInterval? candleInterval = null)
        {
            // Проверка на null перед использованием параметров
            if (ticker == null)
            {
                // Обработка случая, когда 'ticker' не был передан
            }

            if (candleHistoricalIntervalIndex == null)
            {
                // Обработка случая, когда 'candleHistoricalIntervalIndex' не был передан
            }

            if (candleInterval == null)
            {
                // Обработка случая, когда 'candleInterval' не был передан
            }

            // Создаю объект для отображения свечей
            var plotModel = CreateCandlestickPlotModel();

            List<CandlestickData> candlestickData = await TinkoffTradingPrices.GetCandlesData(ticker: ticker, candleHistoricalIntervalIndex: SelectedHistoricalTimeCandleIndex, candleInterval: candleInterval);

            // Очистите старые серии данных из PlotModel
            plotModel.Series.Clear();

            var candlestickSeries = new CandleStickSeries
            {
                Title = "Candlesticks",
                Background = OxyColor.FromAColor(128, OxyColor.Parse("#A5424B51")) // Темный полупрозрачный фон               
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


            plotModel.Series.Add(candlestickSeries);
            PlotModel.Model = plotModel;

            // Обновите PlotModel, чтобы обновить график
            PlotModel.InvalidatePlot(true);
        }
                

        // Метод создания объекта модели отображения свечей
        private PlotModel CreateCandlestickPlotModel()
        {
            var plotModel = new PlotModel { Title = $"График свечей для {Title}" };
            plotModel.Axes.Add(new LinearAxis { IsPanEnabled = true, IsZoomEnabled = false }); // Горизонтальная ось
            plotModel.Axes.Add(new LinearAxis { IsPanEnabled = true, IsZoomEnabled = false, Position = AxisPosition.Bottom }); // Вертикальная ось
            return plotModel;
        }


        // Метод получения выбранной кнопки для отображения имени
        private async void OnCandleIntervalSelected(CandleTimeFrameButton selectedButton)
        {
            SelectedTimeFrame = selectedButton;
            string? intervalName = SelectedTimeFrame.Name;

            CandleInterval candleInterval = (CandleInterval)Enum.Parse(typeof(CandleInterval), intervalName);

            await SetAndUpdateCandlesChartWindow(candleInterval: candleInterval);
        }

        // Метод загрузки асинхронных данных для вызова из конструктора
        public async void LoadAsyncData()
        {
            // Создаю клиента Тинькофф 
            _client = await TinkoffClient.CreateAsync();

            TinkoffTradingPrices tinkoff = new TinkoffTradingPrices(_client);

            // Получаю обновлённый список свечей c задаными параметрами
            // List<CandlestickData> candles = await TinkoffTradingPrices.GetCandlesData(ticker: Title, candleHistoricalIntervalIndex: SelectedHistoricalTimeCandleIndex);

            //UpdateData(candles);
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
        public async void IncreaseCandleHistorical()
        {
            // Максимально допустимый индекс для выбора таймфрейма
            int maxIndex = 100;

            SelectedHistoricalTimeCandleIndex += 10;
            if (SelectedHistoricalTimeCandleIndex > maxIndex) SelectedHistoricalTimeCandleIndex = maxIndex;

            var asdfasd = SelectedHistoricalTimeCandleIndex;
            await SetAndUpdateCandlesChartWindow(candleHistoricalIntervalIndex: SelectedHistoricalTimeCandleIndex);
        }

        // Метод уменьшения таймфрейма свечи
        public async void DecreaseCandleIHistorical()
        {
            // Минимально допустимый индекс для выбора таймфрейма
            int minxIndex = 10;

            SelectedHistoricalTimeCandleIndex -= 10;
            if (SelectedHistoricalTimeCandleIndex < minxIndex) SelectedHistoricalTimeCandleIndex = minxIndex;

            var asdfasd = SelectedHistoricalTimeCandleIndex;

            await SetAndUpdateCandlesChartWindow(candleHistoricalIntervalIndex: SelectedHistoricalTimeCandleIndex);
        }
        // Метод продажи
        #endregion

        #region Методы по торговле (покупка/продажа)
        // Общий метод покупки и продажи
        private void ExecuteTrade(string operation)
        {
            string tickerName = Title;
            double price = 3444;
            bool subtractVolume = operation == "Продажа"; // Проверяем, нужно ли вычитать объем

            // Поиск записи с тем же TickerName в _db.TradeRecordsInfo
            TradeRecordInfo tradeRecordInfo = _db.TradeRecordsInfo.SingleOrDefault(tr => tr.TickerName == tickerName);

            if (tradeRecordInfo != null)
            {
                // Запись найдена, обновляем Volume, Price и Operation
                tradeRecordInfo.Price = price; // Обновляем цену
                tradeRecordInfo.Operation = operation; // Обновляем тип операции

                if (subtractVolume)
                {
                    // Если нужно вычесть объем, убедимся, что объем не становится отрицательным
                    if (tradeRecordInfo.Volume >= VolumeTradingTicker)
                    {
                        tradeRecordInfo.Volume -= VolumeTradingTicker;
                    }
                    else
                    {
                        // Обработка ошибки, если объем торговли меньше, чем пытаемся продать
                        MessageBox.Show("Недостаточно объема для продажи", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return; // Выходим из метода, так как объем недостаточный
                    }
                }
                else
                {
                    tradeRecordInfo.Volume += VolumeTradingTicker; // Пример, можете использовать нужную логику увеличения объема
                }

                if (tradeRecordInfo.Volume == 0)
                {
                    // Если объем стал равным 0, удаляем запись из базы данных
                    _db.TradeRecordsInfo.Remove(tradeRecordInfo);
                }
            }
            else if (subtractVolume)
            {
                // Запись не найдена и мы пытаемся продать, отображаем сообщение об ошибке
                MessageBox.Show("Нет записи для продажи", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else
            {
                // Запись не найдена и мы пытаемся купить, создаем новую запись
                tradeRecordInfo = new TradeRecordInfo
                {
                    TickerName = tickerName,
                    Price = price,
                    IsBuy = operation == "Покупка", // Определение типа операции
                    Operation = operation,
                    Volume = subtractVolume ? -VolumeTradingTicker : VolumeTradingTicker // Устанавливаем объем с учетом операции
                };

                // Добавляем запись в _db.TradeRecordsInfo
                _db.TradeRecordsInfo.Add(tradeRecordInfo);
            }

            // Создаем новую запись для истории торговли
            HistoricalTradeRecordInfo historicalTradeRecordInfo = new HistoricalTradeRecordInfo
            {
                TickerName = tickerName,
                Price = price,
                Operation = operation,
                Volume = VolumeTradingTicker, // Используем значение из VolumeTradingTicker
                Date = DateTime.Now // Устанавливаем дату
            };

            // Добавляем запись в _db.HistoricalTradeRecordsInfo
            _db.HistoricalTradeRecordsInfo.Add(historicalTradeRecordInfo);

            // Сохраняем изменения в базе данных
            _db.SaveChanges();

            // Очищаем и обновляем _tradeHistoricalInfoList
            _tradeHistoricalInfoList.Clear();
            foreach (var item in _db.HistoricalTradeRecordsInfo.ToList())
            {
                _tradeHistoricalInfoList.Add(item);
            }

            // Очищаем и обновляем _tradeCurrentInfoList из базы данных
            _tradeCurrentInfoList.Clear();
            foreach (var item in _db.TradeRecordsInfo.ToList())
            {
                _tradeCurrentInfoList.Add(item);
            }

            // Опубликовываем событие для текущей коллекции
            EventAggregator.PublishTradingInfoChanged(_tradeCurrentInfoList);

            // Опубликовываем событие для исторической коллекции
            EventAggregator.PublishHistoricalTradeInfoChanged(_tradeHistoricalInfoList);
        }

        // Метод для покупки
        private void BuyTicker()
        {
            ExecuteTrade("Покупка");
        }

        // Метод для продажи
        private void SellTicker()
        {
            ExecuteTrade("Продажа");
        }

        #endregion
        // Метод установки стилей для графика

        #endregion
    }
}
