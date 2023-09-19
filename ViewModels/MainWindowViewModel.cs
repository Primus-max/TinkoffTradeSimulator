using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Tinkoff.InvestApi;
using TinkoffTradeSimulator.ApiServices;
using TinkoffTradeSimulator.Data;
using TinkoffTradeSimulator.Infrastacture.Commands;
using TinkoffTradeSimulator.Models;
using TinkoffTradeSimulator.ViewModels.Base;
using TinkoffTradeSimulator.Views.Windows;

namespace TinkoffTradeSimulator.ViewModels
{

    // TODO Выводить стоимость 

    // TODO Создать таблицу с историей сделок. Если будет флаг IsClosed значит это уже история сделок
    // TODO Добавить строку поиска по тикерам
    // TODO Добавить вкладку Избранное, в ней будут храниться тикеры 
    // TODO Реализовать принцип хранения тикеров в избранном

    // -----------------------------------------------------------------
    // TODO Сделать логику покупки и продажи акций тикеров
    // логика должна быть имттацией настоящей торговли, но реальных данных 

    internal class MainWindowViewModel : BaseViewModel
    {
        #region Приватные поля
        private InvestApiClient? _client = null;
        private ObservableCollection<TickerInfo>? _tickerInfoList = null!;
        private ObservableCollection<TradeRecordInfo>? _tradeHistoricalInfoList = null!;
        private ObservableCollection<TradeRecordInfo>? _tradingInfoList = null!;
        private string _title = string.Empty;
        private ChartWindowViewModel _chartViewModel = null!;
        private AppContext _db = null!;
        private TradeRecordInfo? _selectedTradeInfo = null!;
        private decimal _accountBalance = 1000;
        private DateTime _currentDate = DateTime.Now;
        private decimal _dailyEarnings = 1200;
        #endregion

        #region Публичные поля
        public string Title
        {
            get => _title;
            set => Set(ref _title, value);
        }
        public ObservableCollection<TickerInfo> TickerInfoList
        {
            get => _tickerInfoList;
            set => Set(ref _tickerInfoList, value);

        }
        public ObservableCollection<TradeRecordInfo> TradeHistoricalInfoList
        {
            get => _tradeHistoricalInfoList;
            set => Set(ref _tradeHistoricalInfoList, value);
        }

        public ObservableCollection<TradeRecordInfo> TradingInfoList
        {
            get => _tradingInfoList;
            set => Set(ref _tradingInfoList, value);
        }

        public TradeRecordInfo SelectedTradeInfo
        {
            get => _selectedTradeInfo;
            set => Set(ref _selectedTradeInfo, value);
        }
        public decimal AccountBalance
        {
            get => _accountBalance;
            set => Set(ref _accountBalance, value);
        }

        public DateTime CurrentDate
        {
            get => _currentDate;
            set => Set(ref _currentDate, value);
        }
        public decimal DailyEarnings
        {
            get => _dailyEarnings;
            set => Set(ref _dailyEarnings, value);
        }
        #endregion

        #region Команды
        public ICommand OpenChartWindowCommand { get; }

        private bool CanOpenChartWindowCommandExecute(object p) => true;

        private void OnOpenChartWindowCommandExecuted(object sender)
        {
            // Получаю имя тикера из параметра который передаю из view по CommandParameter
            string tickerName = sender?.ToString();

            // Открываю окно
            OpenChartWindow(tickerName);
        }

        public ICommand? DeleteTradingTickerInfoCommand { get; } = null;

        private bool CanDeleteDradingTickerInfoCommandExecute(object p) => true;

        private void OnDeleteDradingTickerInfoCommandExecuted(object sender)
        {
            TradeRecordInfo? tickerInfo = sender as TradeRecordInfo;

            DbManager.Delete(tickerInfo);

        }

        #endregion

        // Конструктор
        public MainWindowViewModel()
        {

            #region Инициализация команд
            OpenChartWindowCommand = new LambdaCommand(OnOpenChartWindowCommandExecuted, CanOpenChartWindowCommandExecute);

            DeleteTradingTickerInfoCommand = new LambdaCommand(OnDeleteDradingTickerInfoCommandExecuted, CanDeleteDradingTickerInfoCommandExecute);
            #endregion

            #region Инициализация базы данных
            DbManager dbManager = new();
            _db = dbManager.InitializeDB();
            #endregion

            // Создаю новую ViewModel для окна
            _chartViewModel = new ChartWindowViewModel();

            _ = LoadDataFromTinkoffApi();
            LoadHistorticalTradingData();
            LoadTradingData();
        }

        #region Методы

        // Загружаю / отображаю актуальные данные из Tinkoff InvestAPI 
        public async Task LoadDataFromTinkoffApi()
        {
            // Создаю клиента Тинькофф 
            _client = await TinkoffClient.CreateAsync();

            // Получаю все актуальные данные от сервера
            var instruments = await _client?.Instruments?.SharesAsync();

            // Инициализирую коллекцию
            TickerInfoList = new ObservableCollection<TickerInfo>();

            // Заполняю данными список который будет источником данных для отображения в главном окне
            foreach (var instrument in instruments.Instruments)
            {
                TickerInfoList.Add(new TickerInfo { Id = instrument.Isin, TickerName = instrument.Ticker });
            }
        }

        // Загружаю / отображаю исторические данные торгов
        private void LoadHistorticalTradingData()
        {
            // Привожу у нужным данным коллекцию из базы данных
            TradeHistoricalInfoList = new ObservableCollection<TradeRecordInfo>(_db.TradeRecordsInfo.ToList());
        }

        // Загружаю / отображаю актуальные (торговые данные)
        private void LoadTradingData()
        {
            // Привожу у нужным данным коллекцию из базы данных
            TradingInfoList = new ObservableCollection<TradeRecordInfo>(_db.TradeRecordsInfo.ToList());
        }

        // Открываю окно и строю в нём график
        private void OpenChartWindow(string tickerName)
        {

            // Создаем новое окно и передаем ему ViewModel
            var chartWindow = new ChartWindow();

            // Устанавливаю значение Title через свойство
            _chartViewModel.Title = tickerName;

            //_chartViewModel.GetAndSetCandlesIntoView(tickerName);
            // Устанавливаю контекст даннх для окна (странно, но это так же делаю в самом окне)
            chartWindow.DataContext = _chartViewModel;

            //_chartViewModel.SetDataToView();

            // Открываем окно
            chartWindow.Show();
        }

        // Метод загрузки базы данных


        #endregion
    }

}
