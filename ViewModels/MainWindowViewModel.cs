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
        private ObservableCollection<TickerInfo>? _tickerInfoList;
        private ObservableCollection<TradeRecordInfo>? _tradeHistoricalInfoList = null!;
        private ObservableCollection<TradeRecordInfo>? _tradingInfoList = null!;
        private string _title = string.Empty;
        private ChartWindowViewModel _chartViewModel = null;
        private AppContext _db = null!;
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

        #endregion


        // Конструктор
        public MainWindowViewModel()
        {
            #region Инициализация команд
            OpenChartWindowCommand = new LambdaCommand(OnOpenChartWindowCommandExecuted, CanOpenChartWindowCommandExecute);
            #endregion

            #region Инициализация базы данных
            DbManager dbManager = new DbManager();
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

            // Загрузите данные из Tinkoff API асинхронно
            await Task.Delay(1000); // Пример задержки, замените на реальную загрузку данных

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
