using ScottPlot;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
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
using Style = ScottPlot.Style;

namespace TinkoffTradeSimulator.ViewModels
{
    internal class ChartWindowViewModel : BaseViewModel
    {

        #region Приватные свойства
        private InvestApiClient? _client = null;
        private WpfPlot? _wpfPlot = null;
        private string _title = string.Empty;
        private AppContext _db = null!;

        // Приватное свойство для определения временно интервала свечей
        private int _selectedHistoricalTimeCandleIndex = 10;

        // Приватное свойство для хранения данных свечей
        private ObservableCollection<OHLC>? _candlestickData = null;
        private CandleTimeFrameButton _selectedTimeFrame = new CandleTimeFrameButton { Name = CandleInterval._1Min.ToString() };
        private int _volumeTradingTicker = 1;
        private ObservableCollection<HistoricalTradeRecordInfo> _tradeHistoricalInfoList = null!;
        private ObservableCollection<TradeRecordInfo> _tradeCurrentInfoList = null!;
        private TickerInfo _stockInfo = null!;
        private string _ticker = string.Empty;        
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
        public string Ticker
        {
            get => _ticker;
            set => Set(ref _ticker, value);
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
            #endregion

            #region Подписки на события
            // Подписываемся на событие CandleIntervalSelected
            EventAggregator.CandleIntervalSelected += OnCandleIntervalSelected;
            #endregion

            StockInfo = new TickerInfo();

            // Тестовые данные
            //StockInfo = new TickerInfo
            //{
            //    Close = "23r",
            //    MaxPrice = "345",
            //    MinPrice = "sfdgdf",
            //    TickerName = "adfgad",
            //    Open = "sdfsfd",
              
            //};
        }

        // Коснтурктор с перегрузами
        public ChartWindowViewModel(WpfPlot plot, string ticker)
        {

            // Делаю доступным в этой области видимости полученный объект из конструктора
            _wpfPlot = plot;


            // Устанавливаю стили для графика
            SetPlotStyle(ticker);


            // TODO найти в чём причина обнуления Title при определённых сценариях
            // Обновляю заголовок окна на актуальный 
            Title = ticker;

            // TODO разобраться какая инициализация лишняя
            #region Инициализация команд
            OpenCandleIntervalWindowCommand = new LambdaCommand(OnOpenCandleIntervalWindowCommandExecuted, CanOpenCandleIntervalWindowCommandExecute);
            #endregion

            #region Инициализация базы данных
            DbManager dbManager = new();
            _db = dbManager.InitializeDB();
            #endregion


            // Загрузка каких-нибудь асинхронных данных
            LoadAsyncData();
        }

        #region Методы

        // Метод получения выбранно кнопки для отображения имени
        private void OnCandleIntervalSelected(CandleTimeFrameButton selectedButton)
        {
            SelectedTimeFrame = selectedButton;
        }

        // Метод загрузки асинхронных данных для вызова из конструктора
        private  async void LoadAsyncData()
        {
            // Создаю клиента Тинькофф 
            _client = await TinkoffClient.CreateAsync();

            

            // Получаю обновлённый список свечей c задаными параметрами
            OHLC[] pricesArray = await TinkoffTradingPrices.GetCandlesData(ticker: Title, candleHistoricalIntervalIndex: SelectedHistoricalTimeCandleIndex);

            Share instrument = await TinkoffTradingPrices.GetShareByTicker(Title);

            Ticker = instrument.Ticker;

            
            // Устанавливаю данные в окно (цены, максимальная, минимальная и т.д.)            
            // await SetStockInfo(pricesArray);

            // Устанавливаю данные для для окна (график свечей)
            UpdateChartWindow(pricesArray);
        }


        // Метод для установки значени в окне (Прайс, минимальная, максимальная)
        private async Task SetStockInfo(OHLC[] pricesArray)
        {
            if (pricesArray == null || pricesArray.Length == 0)
            {
                // Если массив свечей пуст, выход из метода
                return;
            }

            // Получаем первую свечу (первое значение в массиве)
            OHLC firstCandle = pricesArray[0];

            // Получаем максимальную и минимальную цену из всех свечей
            decimal maxPrice = (decimal)pricesArray.Max(candle => candle.High);
            decimal minPrice = (decimal)pricesArray.Min(candle => candle.Low);

            // Далее вы можете получить данные о валюте и стоимости акции
            Share instrument = await TinkoffTradingPrices.GetShareByTicker(Title);
            

            // Создаем объект TickerInfo и заполняем его данными
            TickerInfo tickerInfo = new TickerInfo
            {
                TickerName = instrument.Ticker,
                Open = firstCandle.Open.ToString("C"), // Форматируем валюту
                Close = firstCandle.Close.ToString("C"), // Форматируем валюту
               // Price = instrument.Price.ToString("C"), // Форматируем валюту
                                                        // Здесь вы также можете добавить данные о максимальной и минимальной цене
                MaxPrice = maxPrice.ToString("C"), // Форматируем валюту
                MinPrice = minPrice.ToString("C") // Форматируем валюту
            };

            // Устанавливаем TickerInfo как свойство вашей ViewModel
            StockInfo = tickerInfo;
        }


        // Метод обновления окна с данными
        public void UpdateChartWindow(OHLC[] prices)
        {
            // Очищаем текущий график перед добавлением новых свечей
            _wpfPlot?.Plot.Clear();

            // Добавляю полученный список через специальный метод ScottPlot
            _wpfPlot?.Plot.AddCandlesticks(prices);

            // Обновляю view
            _wpfPlot?.Refresh();
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
        public async void IncreaseCandleHistorical()
        {
            // Максимально допустимый индекс для выбора таймфрейма
            int maxIndex = 100;

            SelectedHistoricalTimeCandleIndex += 10;
            if (SelectedHistoricalTimeCandleIndex > maxIndex) SelectedHistoricalTimeCandleIndex = maxIndex;

            await TinkoffTradingPrices.GetCandlesData(Title, SelectedHistoricalTimeCandleIndex);
        }

        // Метод уменьшения таймфрейма свечи
        public async void DecreaseCandleIHistorical()
        {
            // Минимально допустимый индекс для выбора таймфрейма
            int minxIndex = 10;

            SelectedHistoricalTimeCandleIndex -= 10;
            if (SelectedHistoricalTimeCandleIndex < minxIndex) SelectedHistoricalTimeCandleIndex = minxIndex;

            await TinkoffTradingPrices.GetCandlesData(Title, SelectedHistoricalTimeCandleIndex);
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
        private void SetPlotStyle(string tickerName)
        {
            _wpfPlot.Plot.Style(Style.Gray1);
            _wpfPlot.Plot.Title($"График свечей для {tickerName}");
            _wpfPlot.Plot.XLabel("Ось Х");
            _wpfPlot.Plot.YLabel("Ось Y");
        }

        #endregion
    }
}
