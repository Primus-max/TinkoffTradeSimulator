using ScottPlot;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Tinkoff.InvestApi;
using Tinkoff.InvestApi.V1;
using TinkoffTradeSimulator.ApiServices;
using TinkoffTradeSimulator.ApiServices.Tinkoff;
using TinkoffTradeSimulator.Data;
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
        private AppContext _db = null!;

        // Приватное свойство для определения временно интервала свечей
        private int _selectedHistoricalTimeCandleIndex = 10;

        // Приватное свойство для хранения данных свечей
        private ObservableCollection<OHLC>? _candlestickData = null;     

        private CandleTimeFrameButton _selectedTimeFrame = new CandleTimeFrameButton { Name = CandleInterval._1Min.ToString()};
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
            double value = Convert.ToDouble(sender);

            BuyTicker(value);
        }

        
        #endregion

        // Пустой (необходимый) конструктор
        public ChartWindowViewModel()
        {
            // TODO разобраться какая инициализация лишняя
            #region Инициализация команд
            OpenCandleIntervalWindowCommand = new LambdaCommand(OnOpenCandleIntervalWindowCommandExecuted, CanOpenCandleIntervalWindowCommandExecute);

            BuyTickerCommand = new LambdaCommand(OnBuyTickerCommandExecuted, CanBuyTickerCommandExecute);            
            #endregion

            #region Инициализация базы данных
            DbManager dbManager = new();
            _db = dbManager.InitializeDB();
            #endregion

            #region Подписки на события
            // Подписываемся на событие CandleIntervalSelected
            EventAggregator.CandleIntervalSelected += OnCandleIntervalSelected;
            #endregion
        }
        
        // Коснтурктор с перегрузами
        public ChartWindowViewModel(WpfPlot plot, string ticker)
        {

            // Делаю доступным в этой области видимости полученный объект из конструктора
            _wpfPlot = plot;

            // TODO найти в чём причина обнуления Title при определённых сценариях
            // Обновляю заголовок окна на актуальный 
            Title = ticker;

            // TODO разобраться какая инициализация лишняя
            #region Инициализация команд
            OpenCandleIntervalWindowCommand = new LambdaCommand(OnOpenCandleIntervalWindowCommandExecuted, CanOpenCandleIntervalWindowCommandExecute);
            #endregion

            #region Инициализация базы данных
            DbManager dbManager = new ();
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
        private async void LoadAsyncData()
        {
            // Создаю клиента Тинькофф 
            _client = await TinkoffClient.CreateAsync();

            // Получаю обновлённый список свечей c задаными параметрами
            OHLC[] pricesArray = await TinkoffTradingPrices.GetCandlesData(ticker: Title, candleHistoricalIntervalIndex: SelectedHistoricalTimeCandleIndex);
            // Получаю данные по тикерам (передаю именованные параметры)
             UpdateChartWindow(pricesArray);
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

        // Метод покупки 
        private void BuyTicker(double value)
        {
            TradeRecordInfo tradeRecordInfo = new TradeRecordInfo();

            tradeRecordInfo.Price = 3444;
            tradeRecordInfo.TickerName = Title;
            tradeRecordInfo.IsBuy = true;
            tradeRecordInfo.Operation = "Продажа";

            DbManager.SaveData(tradeRecordInfo);
        }

        #endregion
    }
}
