using ScottPlot;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Tinkoff.InvestApi;
using TinkoffTradeSimulator.ApiServices;
using TinkoffTradeSimulator.ApiServices.Tinkoff;
using TinkoffTradeSimulator.Infrastacture.Commands;
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

        // Приватное свойство для определения временно интервала свечей
        private int _selectedHistoricalTimeCandleIndex = 10;

        // Приватное свойство для хранения данных свечей
        private ObservableCollection<OHLC>? _candlestickData = null;

        private ChartToolTipManager? _сhartToolTipManager = null;

        private ToolTip? _toolTipInfo = null;


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

        public ChartToolTipManager ChartToolTipManager
        {
            get => _сhartToolTipManager;
            set => Set(ref _сhartToolTipManager, value);
        }

        public ToolTip ToolTipInfo
        {
            get => _toolTipInfo;
            set => Set(ref _toolTipInfo, value);
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
        #endregion

        // Пустой (необходимый) конструктор
        public ChartWindowViewModel()
        {
            // TODO разобраться какая инициализация лишняя
            #region Инициализация команд
            OpenCandleIntervalWindowCommand = new LambdaCommand(OnOpenCandleIntervalWindowCommandExecuted, CanOpenCandleIntervalWindowCommandExecute);
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
                       

            // Загрузка каких-нибудь асинхронных данных
            LoadAsyncData();
        }

        #region Методы
        
        // Метод загрузки асинхронных данных для вызова из конструктора
        private async void LoadAsyncData()
        {
            // Создаю клиента Тинькофф 
            _client = await TinkoffClient.CreateAsync();

            // Получаю обновлённый список свечей
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

            SelectedHistoricalTimeCandleIndex++;
            if (SelectedHistoricalTimeCandleIndex > maxIndex) SelectedHistoricalTimeCandleIndex = maxIndex;

            await TinkoffTradingPrices.GetCandlesData(Title, SelectedHistoricalTimeCandleIndex);
        }

        // Метод уменьшения таймфрейма свечи
        public async void DecreaseCandleIHistorical()
        {
            // Минимально допустимый индекс для выбора таймфрейма
            int minxIndex = 1;

            SelectedHistoricalTimeCandleIndex--;
            if (SelectedHistoricalTimeCandleIndex < minxIndex) SelectedHistoricalTimeCandleIndex = minxIndex;

            await TinkoffTradingPrices.GetCandlesData(Title, SelectedHistoricalTimeCandleIndex);
        }
        #endregion

        #endregion
    }
}
