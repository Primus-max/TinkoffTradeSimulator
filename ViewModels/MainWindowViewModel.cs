using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Tinkoff.InvestApi;
using TinkoffTradeSimulator.ApiServices;
using TinkoffTradeSimulator.Data;
using TinkoffTradeSimulator.Infrastacture.Commands;
using TinkoffTradeSimulator.Models;
using TinkoffTradeSimulator.Models.Interfaces;
using TinkoffTradeSimulator.Services;
using TinkoffTradeSimulator.ViewModels.Base;
using TinkoffTradeSimulator.Views.Windows;

namespace TinkoffTradeSimulator.ViewModels
{

    // TODO Выводить стоимость    
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
        private ObservableCollection<HistoricalTradeRecordInfo>? _tradeHistoricalInfoList = null!;
        private ObservableCollection<HistoricalTradeRecordInfo>? _filteredTradeHistoricalInfoList = null!;
        private ObservableCollection<TradeRecordInfo>? _tradingInfoList = null!;
        private string _title = string.Empty;
        private ChartWindowViewModel _chartViewModel = null!;
        private AppContext _db = null!;
        private TradeRecordInfo? _selectedTradeInfo = null!;
        private decimal _accountBalance = 1000;
        private DateTime _currentDate = DateTime.Now;
        private decimal _dailyEarnings = 1200;
        private string _filterByTickerNameAll = string.Empty;
        private readonly ObservableCollection<TickerInfo> _originalTickerInfoList = new ObservableCollection<TickerInfo>();
        private string _filterByTickerTradeRecordHistorical = string.Empty;
        private ObservableCollection<object> _originalTradeInfoList = new ObservableCollection<object>();

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
        public ObservableCollection<HistoricalTradeRecordInfo> TradeHistoricalInfoList
        {
            get => _tradeHistoricalInfoList;
            set => Set(ref _tradeHistoricalInfoList, value);
        }
        public ObservableCollection<HistoricalTradeRecordInfo> FilteredTradeHistoricalInfoList
        {
            get => _filteredTradeHistoricalInfoList;
            set => Set(ref _filteredTradeHistoricalInfoList, value);
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
        public string FilterByTickerNameAll
        {
            get => _filterByTickerNameAll;
            set => Set(ref _filterByTickerNameAll, value);
        }
        public string FilterByTickerTradeRecordHistorical
        {
            get => _filterByTickerTradeRecordHistorical;
            set => Set(ref _filterByTickerTradeRecordHistorical, value);
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
            UpdateFilteredTickerInfoList(FilterByTickerNameAll);
        }

        public ICommand? FilterTradingRecorsInfoListCommand { get; } = null;

        private bool CanFilterTradingRecorsInfoListCommandExecute(object p) => true;

        private void OnFilterTradingRecorsInfoListCommandExecuted(object sender)
        {
            //TradeHistoricalInfoList = FilterTradeInfoListByTicker(TradeHistoricalInfoList, FilterByTickerTradeRecordHistorical);
        }
        #endregion

        // Конструктор
        public MainWindowViewModel()
        {

            #region Инициализация команд
            OpenChartWindowCommand = new LambdaCommand(OnOpenChartWindowCommandExecuted, CanOpenChartWindowCommandExecute);

            CloseTradingDealCommand = new LambdaCommand(OnCloseTradingDealCommandExecuted, CanCloseTradingDealCommandExecute);

            FilterTickerInfoListCommand = new LambdaCommand(OnFilterTickerInfoListCommandCommandExecuted, CanFilterTickerInfoListCommandCommandExecute);

            FilterTradingRecorsInfoListCommand = new LambdaCommand(OnFilterTradingRecorsInfoListCommandExecuted, CanFilterTradingRecorsInfoListCommandExecute);
            #endregion

            #region Инициализация базы данных
            DbManager dbManager = new();
            _db = dbManager.InitializeDB();
            #endregion

            #region Инициализация источников данных
            FilteredByTickerInfoList = new ObservableCollection<TickerInfo>();
            TradingInfoList = new ObservableCollection<TradeRecordInfo>(); // Коллекция о текущих операциях
            TradeHistoricalInfoList = new ObservableCollection<HistoricalTradeRecordInfo>(); // Коллекция об исторических операциях
            FilteredTradeHistoricalInfoList = new ObservableCollection<HistoricalTradeRecordInfo>(); // Отфильтрованная коллекция
            #endregion

            _chartViewModel = new ChartWindowViewModel();

            #region Загрузка источников данных
            new Thread(async () => await LoadDataFromTinkoffApi()).Start();
            LoadHistorticalTradingData();
            LoadTradingData();
            #endregion

            #region Подписчики на события
            EventAggregator.HistoricalTradeInfoChanged += OnHistoricalTradeInfoChanged;
            EventAggregator.TradingRecordInfoChanged += OnTradingInfoListChanged;
            #endregion
        }

        #region Методы
        #region Метод вызывающиеся подписчиками на события
        // Метод оповещения об изменении источника данных для отображения во View
        private void OnTradingInfoListChanged(ObservableCollection<TradeRecordInfo> tradingIndolist)
        {
            TradingInfoList = tradingIndolist;
        }
        // Метод оповещения об изменении источника данных для отображения во View
        private void OnHistoricalTradeInfoChanged(ObservableCollection<HistoricalTradeRecordInfo> historicalTradeRecord)
        {
            TradeHistoricalInfoList = historicalTradeRecord;
        }
        #endregion

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
            TradeHistoricalInfoList = new ObservableCollection<HistoricalTradeRecordInfo>(_db.HistoricalTradeRecordsInfo.ToList());
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

        #region Фильтры
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

        // Обобщенный метод для фильтрации
        public ObservableCollection<T> FilterTradeInfoListByTicker<T>(string tickerNameFilter) where T : ITradeInfo
        {
            if (string.IsNullOrWhiteSpace(tickerNameFilter))
            {
                // Если фильтр пустой или null, возвращаем исходную коллекцию без фильтрации
                return new ObservableCollection<T>(_originalTradeInfoList.OfType<T>());
            }

            // Очищаем фильтрованную коллекцию
            var filteredList = new ObservableCollection<T>();

            foreach (var item in _originalTradeInfoList.OfType<T>())
            {
                if (item.TickerName.Contains(tickerNameFilter, StringComparison.OrdinalIgnoreCase))
                {
                    // Если элемент соответствует фильтру, добавляем его в фильтрованную коллекцию
                    filteredList.Add(item);
                }
            }

            return filteredList;
        }
        public void FilterTradeHistoricalInfoListByTicker(string tickerNameFilter)
        {
            // Используем обобщенный метод для фильтрации TradeHistoricalInfoList
            TradeHistoricalInfoList = FilterTradeInfoListByTicker<HistoricalTradeRecordInfo>(tickerNameFilter);
        }

        public void FilterTradingInfoListByTicker(string tickerNameFilter)
        {
            // Используем обобщенный метод для фильтрации TradingInfoList
            TradingInfoList = FilterTradeInfoListByTicker<TradeRecordInfo>(tickerNameFilter);
        }

    }

    #endregion


    #endregion
}


#endregion