using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Tinkoff.InvestApi;
using TinkoffTradeSimulator.ApiServices;
using TinkoffTradeSimulator.Infrastacture.Commands;
using TinkoffTradeSimulator.Models;
using TinkoffTradeSimulator.ViewModels.Base;
using TinkoffTradeSimulator.Views.Windows;

namespace TinkoffTradeSimulator.ViewModels
{
    
    // TODO Выводить стоимость 
    
    // TODO подключить базу данных SQLite
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
            InitializeDB();
            #endregion

            // Создаю новую ViewModel для окна
            _chartViewModel = new ChartWindowViewModel();

            _ = LoadData();
        }

        #region Методы

        // Загружаю актуальные данные из Tinkoff InvestAPI 
        public async Task LoadData()
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
        private void InitializeDB()
        {
            // Экземпляр базы данных
            _db = new AppContext();

            // гарантируем, что база данных создана
            _db.Database.EnsureCreated();
            // загружаем данные из БД
            _db.TradeRecordsInfo.Load();
        }
        
        #endregion
    }

}
