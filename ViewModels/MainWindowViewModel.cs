using DromAutoTrader.Views;
using System;
using System.Collections.Generic;
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
using TinkoffTradeSimulator.Services;
using TinkoffTradeSimulator.ViewModels.Base;
using TinkoffTradeSimulator.Views.Windows;

namespace TinkoffTradeSimulator.ViewModels
{        
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
        private ObservableCollection<HistoricalTradeRecordInfo> _originalHistoricalTradeRecordInfoList = new ObservableCollection<HistoricalTradeRecordInfo>();
        private ObservableCollection<TradeRecordInfo>? _originalTradingRecordInfoList = new ObservableCollection<TradeRecordInfo>();
        private ObservableCollection<FavoriteTicker> _favoriteTickers = null!;
        private AppConfig _appConfig = null!;
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
        public ObservableCollection<HistoricalTradeRecordInfo> TradeHistoricalInfoList
        {
            get => _tradeHistoricalInfoList;
            set
            {
                if (Set(ref _tradeHistoricalInfoList, value))
                {
                    _tradeHistoricalInfoList = new ObservableCollection<HistoricalTradeRecordInfo>(
                        _tradeHistoricalInfoList.OrderByDescending(info => info.Date));
                    OnPropertyChanged(nameof(TradeHistoricalInfoList));
                }
            }
        }
        public ObservableCollection<HistoricalTradeRecordInfo> FilteredTradeHistoricalInfoList
        {
            get => _filteredTradeHistoricalInfoList;
            set => Set(ref _filteredTradeHistoricalInfoList, value);
        }
        public ObservableCollection<TradeRecordInfo> TradingInfoList
        {
            get => _tradingInfoList;
            set
            {
                if (Set(ref _tradingInfoList, value))
                {
                    _tradingInfoList = new ObservableCollection<TradeRecordInfo>(
                        _tradingInfoList.OrderByDescending(info => info.Date));
                    OnPropertyChanged(nameof(TradingInfoList));
                }
            }
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
        public ObservableCollection<FavoriteTicker> FavoriteTickers
        {
            get => _favoriteTickers;
            set => Set(ref _favoriteTickers, value);
        }
        public AppConfig AppConfig
        {
            get => _appConfig;
            set => Set(ref _appConfig, value);
        }
        #endregion

        #region Команды
        public ICommand OpenChartWindowCommand { get; }

        private bool CanOpenChartWindowCommandExecute(object p) => true;

        private void OnOpenChartWindowCommandExecuted(object sender)
        {
            // Получаю имя тикера из параметра который передаю из view по CommandParameter
            string? tickerName = sender?.ToString();

            // Открываю окно
            OpenChartWindow(tickerName);
        }

        public ICommand? CloseTradingDealCommand { get; } = null;

        private bool CanCloseTradingDealCommandExecute(object p) => true;

        private void OnCloseTradingDealCommandExecuted(object sender)
        {
            // Закрываю сделку

            if (sender is TradeRecordInfo tradeRecordInfo)
            {
                string tickerName = tradeRecordInfo.TickerName;
                double tickerPrice = tradeRecordInfo.Price;
                int volumeTradingTicker = tradeRecordInfo.Volume;

                TradeManager.SellTicker(tickerName, tickerPrice, volumeTradingTicker);

                UpdateTradingInfoAfterExecuteTrade();


                //// Нахожу объект TradeRecordInfo в базе данных и удаляю его
                //var tradeRecordToDelete = _db.TradeRecordsInfo.SingleOrDefault(tr => tr.Id == tradeRecordInfo.Id);
                //if (tradeRecordToDelete != null)
                //{
                //    _db.TradeRecordsInfo.Remove(tradeRecordToDelete);
                //    _db.SaveChanges();

                //    TradingInfoList = new ObservableCollection<TradeRecordInfo>(_db.TradeRecordsInfo.ToList());
                //}

                //// Создаю объект HistoricalTradeRecordInfo и копирую данные
                //var historicalTradeRecordInfo = new HistoricalTradeRecordInfo
                //{
                //    Date = DateTime.Now,
                //    TickerName = tradeRecordInfo.TickerName,
                //    Price = tradeRecordInfo.Price,
                //    Operation = tradeRecordInfo.Operation,
                //    Volume = tradeRecordInfo.Volume,
                //    IsBuy = tradeRecordInfo.IsBuy,
                //    IsSell = tradeRecordInfo.IsSell,
                //    IsClosed = tradeRecordInfo.IsClosed
                //};

                //// Добавляю новый объект HistoricalTradeRecordInfo в базу данных
                //_db.HistoricalTradeRecordsInfo.Add(historicalTradeRecordInfo);
                //_db.SaveChanges();

                //// Обновляю колллекцию для отобажения 
                //TradeHistoricalInfoList = new ObservableCollection<HistoricalTradeRecordInfo>(_db.HistoricalTradeRecordsInfo.ToList());
            }
                        
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

        public ICommand? AddTickerToFavoriteCommand { get; } = null;

        private bool CanAddTickerToFavoriteCommandExecute(object p) => true;

        private void OnAddTickerToFavoriteCommandExecuted(object sender)
        {
            string tickerName = sender?.ToString();
            AddTickerToFavorite(tickerName);
        }

        public ICommand? RemoveTickerToFavoriteCommand { get; } = null;

        private bool CanRemoveTickerToFavoriteCommandExecute(object p) => true;

        private void OnRemoveTickerToFavoriteCommandExecuted(object sender)
        {
            string tickerName = sender?.ToString();
            RemoveFavoriteTickerByName(tickerName);
        }

        public ICommand? SaveSettingsCommand { get; } = null;

        private bool CanSaveSettingsCommandExecute(object p) => true;

        private void OnSaveSettingsCommandExecuted(object sender)
        {
            var asdf = AppConfig;
            SaveSettings();
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
            AddTickerToFavoriteCommand = new LambdaCommand(OnAddTickerToFavoriteCommandExecuted, CanAddTickerToFavoriteCommandExecute);
            RemoveTickerToFavoriteCommand = new LambdaCommand(OnRemoveTickerToFavoriteCommandExecuted, CanRemoveTickerToFavoriteCommandExecute);
            SaveSettingsCommand = new LambdaCommand(OnSaveSettingsCommandExecuted, CanSaveSettingsCommandExecute);
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
            FavoriteTickers = new ObservableCollection<FavoriteTicker>(); // Коллекия с избранными тикерами
            #endregion

            _chartViewModel = new ChartWindowViewModel();

            #region Загрузка источников данных
            new Thread(async () => await LoadFavoriteTickers()).Start();
            LoadHistorticalTradingData();
            LoadTradingData();
            GetSettings();
            //LoadFavoriteTickers();
            #endregion

            #region Подписчики на события
            EventAggregator.HistoricalTradeInfoChanged += OnHistoricalTradeInfoChanged;
            EventAggregator.TradingRecordInfoChanged += OnTradingInfoListChanged;
            #endregion
        }

        #region Методы
        #region Методы подписчиков на события
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

        #region Методы загрузки данных при инициализации приложения        

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
            // Привожу к нужным данным коллекцию из базы данных
            _originalHistoricalTradeRecordInfoList = new ObservableCollection<HistoricalTradeRecordInfo>(
                _db.HistoricalTradeRecordsInfo
                   .OrderByDescending(info => info.Date)
                   .ToList());

            UpdateFilterTradeHistoricalInfoListByTicker(string.Empty);
        }

        // Загружаю / отображаю актуальные (торговые данные)
        private void LoadTradingData()
        {
            // Привожу к нужным данным коллекцию из базы данных
            _originalTradingRecordInfoList = new ObservableCollection<TradeRecordInfo>(
                _db.TradeRecordsInfo
                   .OrderByDescending(info => info.Date)
                   .ToList());

            UpdateFilterTradingInfoListByTicker(string.Empty);
        }

        private async Task LoadFavoriteTickers()
        {
            // Здесь вызываю метод получения тикеров чтобы гарантированно метод LoadFavoriteTickers отработал после получения всех тикеров
            await LoadDataFromTinkoffApi();

            // Получаем список избранных тикеров из базы данных (или откуда у вас они хранятся)
            List<FavoriteTicker> favoriteTickersFromDatabase = _db.FavoriteTickers.ToList();

            // Получаем список всех доступных тикеров (в вашем случае, _originalTickerInfoList)
            List<TickerInfo> allTickers = _originalTickerInfoList.ToList();

            // Создаем новую коллекцию FavoriteTickers на основе совпадающих тикеров из allTickers и favoriteTickersFromDatabase
            var favoriteTickersWithIds = from favorite in favoriteTickersFromDatabase
                                         join ticker in allTickers on favorite.Name equals ticker.TickerName
                                         select new FavoriteTicker
                                         {
                                             Name = favorite.Name,
                                             UId = ticker.Id
                                         };

            // Заполняем коллекцию FavoriteTickers из совпадающих тикеров
            FavoriteTickers = new ObservableCollection<FavoriteTicker>(favoriteTickersWithIds);
        }

        #endregion

        // Метод обновления источников данных после покупки или продажи тикеров
        private void UpdateTradingInfoAfterExecuteTrade()
        {
            // Очищаем и обновляем _tradeHistoricalInfoList
            TradeHistoricalInfoList.Clear();
            foreach (var item in _db.HistoricalTradeRecordsInfo.ToList())
            {
                TradeHistoricalInfoList.Add(item);
            }

            // Очищаем и обновляем _tradeCurrentInfoList из базы данных
            TradingInfoList.Clear();
            foreach (var item in _db.TradeRecordsInfo.ToList())
            {
                TradingInfoList.Add(item);
            }

            // Опубликовываем событие для текущей коллекции
            EventAggregator.PublishTradingInfoChanged(TradingInfoList);

            // Опубликовываем событие для исторической коллекции
            EventAggregator.PublishHistoricalTradeInfoChanged(TradeHistoricalInfoList);
        }

        // Метод сохранения настроек приложения
        private void SaveSettings()
        {
            AppSettings.SaveConfig(AppConfig);
        }
        
        // Метод получения настроек приложения
        private void GetSettings()
        {
          AppConfig = AppSettings.LoadConfig();
        }

        // Добавляю тикер в избранное
        private void AddTickerToFavorite(string? tickerName)
        {
            try
            {
                // Получаем список избранных тикеров из базы данных (или откуда у вас они хранятся)
                List<FavoriteTicker> favoriteTickersFromDatabase = _db.FavoriteTickers.ToList();

                // Проверяем, есть ли уже тикер с таким именем в избранных
                if (favoriteTickersFromDatabase.Any(ticker => ticker.Name == tickerName))
                {
                    // Если тикер уже есть, то не добавляем его повторно
                    return;
                }

                // Получаем список всех доступных тикеров (в вашем случае, _originalTickerInfoList)
                List<TickerInfo> allTickers = _originalTickerInfoList.ToList();

                // Находим тикер в общем списке по имени
                TickerInfo matchingTicker = allTickers.FirstOrDefault(ticker => ticker.TickerName == tickerName);

                if (matchingTicker != null)
                {
                    // Если нашли совпадение, то создаем FavoriteTicker и добавляем его в базу данных
                    FavoriteTicker favoriteTicker = new FavoriteTicker
                    {
                        Name = tickerName,
                        UId = matchingTicker.Id
                    };

                    _db.FavoriteTickers.Add(favoriteTicker);
                    _db.SaveChanges();

                    // Обновляем коллекцию FavoriteTickers после добавления тикера в базу
                    favoriteTickersFromDatabase.Add(favoriteTicker);

                    // Создаем новую коллекцию FavoriteTickers на основе совпадающих тикеров из allTickers и favoriteTickersFromDatabase
                    var favoriteTickersWithIds = from favorite in favoriteTickersFromDatabase
                                                 join ticker in allTickers on favorite.Name equals ticker.TickerName
                                                 select new FavoriteTicker
                                                 {
                                                     Name = favorite.Name,
                                                     UId = ticker.Id
                                                 };

                    // Заполняем коллекцию FavoriteTickers из совпадающих тикеров
                    FavoriteTickers = new ObservableCollection<FavoriteTicker>(favoriteTickersWithIds);
                }
            }
            catch (Exception)
            {
                // Обработка ошибки
            }
        }

        // Удаляю тикер из избранного
        public void RemoveFavoriteTickerByName(string tickerName)
        {
            // Найдите тикер в базе данных по имени
            var tickerToDelete = _db.FavoriteTickers.FirstOrDefault(t => t.Name == tickerName);

            if (tickerToDelete != null)
            {
                try
                {
                    // Если тикер найден, удалите его из базы данных
                    _db.FavoriteTickers.Remove(tickerToDelete);
                    _db.SaveChanges();

                    FavoriteTickers = new ObservableCollection<FavoriteTicker>(_db.FavoriteTickers.ToList());
                }
                catch (Exception)
                {

                }
            }
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

        #region Сделки
        
        #endregion

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

        // Метод фильтрации по историческим сделкам        
        public void UpdateFilterTradeHistoricalInfoListByTicker(string tickerNameFilter)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                TradeHistoricalInfoList.Clear();

                if (string.IsNullOrWhiteSpace(tickerNameFilter))
                {
                    foreach (var tickerInfo in _originalHistoricalTradeRecordInfoList)
                    {
                        TradeHistoricalInfoList.Add(tickerInfo);
                    }
                }
                else
                {
                    foreach (var tickerInfo in _originalHistoricalTradeRecordInfoList.Where(info => info.TickerName.Contains(tickerNameFilter, StringComparison.OrdinalIgnoreCase)))
                    {
                        TradeHistoricalInfoList.Add(tickerInfo);
                    }
                }
            });
        }

        // Метод фильтрации по открытым сделкам
        public void UpdateFilterTradingInfoListByTicker(string tickerNameFilter)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                TradingInfoList.Clear();

                if (string.IsNullOrWhiteSpace(tickerNameFilter))
                {
                    foreach (var tickerInfo in _originalTradingRecordInfoList)
                    {
                        TradingInfoList.Add(tickerInfo);
                    }
                }
                else
                {
                    foreach (var tickerInfo in _originalTradingRecordInfoList.Where(info => info.TickerName.Contains(tickerNameFilter, StringComparison.OrdinalIgnoreCase)))
                    {
                        TradingInfoList.Add(tickerInfo);
                    }
                }
            });
        }
        #endregion
        #endregion
    }
}


