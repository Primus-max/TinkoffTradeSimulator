using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
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
        private ObservableCollection<TickerInfo>? _filteredByTickerInfoList = null!;
        private ObservableCollection<TradeRecordInfo>? _tradeHistoricalInfoList = null!;
        private ObservableCollection<TradeRecordInfo>? _tradingInfoList = null!;
        private string _title = string.Empty;
        private ChartWindowViewModel _chartViewModel = null!;
        private AppContext _db = null!;
        private TradeRecordInfo? _selectedTradeInfo = null!;
        private decimal _accountBalance = 1000;
        private DateTime _currentDate = DateTime.Now;
        private decimal _dailyEarnings = 1200;
        private string _filterByTickerName = string.Empty;     
        private readonly ObservableCollection<TickerInfo> _originalTickerInfoList = new ObservableCollection<TickerInfo>();
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

        public ObservableCollection<TickerInfo> FilteredByTickerInfoList
        {
            get => _filteredByTickerInfoList;
            set => Set(ref _filteredByTickerInfoList, value);

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
        public string FilterByTickerName
        {
            get => _filterByTickerName;
            set => Set(ref _filterByTickerName, value);
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

        public ICommand? CloseTradingDealCommand { get; } = null;

        private bool CanCloseTradingDealCommandExecute(object p) => true;

        private void OnCloseTradingDealCommandExecuted(object sender)
        {
            TradeRecordInfo? tickerInfo = sender as TradeRecordInfo;

            DbManager.Delete(tickerInfo);

        }
        public ICommand? FilterTickerInfoListCommand { get; } = null;

        private bool CanFilterTickerInfoListCommandCommandExecute(object p) => true;

        private void OnFilterTickerInfoListCommandCommandExecuted(object sender)
        {
            UpdateFilteredTickerInfoList(FilterByTickerName);
        }
        #endregion

        // Конструктор
        public MainWindowViewModel()
        {

            #region Инициализация команд
            OpenChartWindowCommand = new LambdaCommand(OnOpenChartWindowCommandExecuted, CanOpenChartWindowCommandExecute);

            CloseTradingDealCommand = new LambdaCommand(OnCloseTradingDealCommandExecuted, CanCloseTradingDealCommandExecute);

            FilterTickerInfoListCommand = new LambdaCommand(OnFilterTickerInfoListCommandCommandExecuted, CanFilterTickerInfoListCommandCommandExecute);
            #endregion

            #region Инициализация базы данных
            DbManager dbManager = new();
            _db = dbManager.InitializeDB();
            #endregion

            #region Инициализация источников данных
            FilteredByTickerInfoList = new ObservableCollection<TickerInfo>();
            TradingInfoList = new ObservableCollection<TradeRecordInfo>();
            #endregion

            
            _chartViewModel = new ChartWindowViewModel();


            #region Загрузка источников данных
            new Thread(async () => await LoadDataFromTinkoffApi()).Start();
            LoadHistorticalTradingData();
            LoadTradingData(); 
            #endregion
        }

        #region Методы
        // Загружаю / отображаю актуальные данные из Tinkoff InvestAPI 
        public async Task LoadDataFromTinkoffApi()
        {
            // Создаю клиента Тинькофф 
            _client = await TinkoffClient.CreateAsync();

            // Получаю все актуальные данные от сервера
            var instruments = await _client?.Instruments?.SharesAsync();

            // Очищаю исходную коллекцию
            _originalTickerInfoList.Clear();

            // Заполняю исходную коллекцию данными
            foreach (var instrument in instruments.Instruments)
            {
                _originalTickerInfoList.Add(new TickerInfo { Id = instrument.Isin, TickerName = instrument.Ticker });
            }

            // Заполняю коллекцию для отображения
            UpdateFilteredTickerInfoList(string.Empty);
        }

        // Загружаю / отображаю исторические данные торгов
        private void LoadHistorticalTradingData()
        {
            // Привожу у нужным данным коллекцию из базы данных
            TradeHistoricalInfoList = new ObservableCollection<TradeRecordInfo>(_db.HIstoricalTradeRecordsInfo.ToList());
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

        // Метод фильтрации по имени тикера
        public void UpdateFilteredTickerInfoList(string filter)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                FilteredByTickerInfoList.Clear();

                if (string.IsNullOrWhiteSpace(filter))
                {
                    foreach (var tickerInfo in _originalTickerInfoList)
                    {
                        FilteredByTickerInfoList.Add(tickerInfo);
                    }
                }
                else
                {
                    foreach (var tickerInfo in _originalTickerInfoList.Where(info => info.TickerName.Contains(filter, StringComparison.OrdinalIgnoreCase)))
                    {
                        FilteredByTickerInfoList.Add(tickerInfo);
                    }
                }
            });
        }

        #endregion
    }

}
