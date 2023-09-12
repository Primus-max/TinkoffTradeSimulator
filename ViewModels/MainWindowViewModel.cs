using System.Collections.ObjectModel;
using System.Threading.Tasks;
using TinkoffTradeSimulator.ApiServices;
using TinkoffTradeSimulator.Models;
using TinkoffTradeSimulator.ViewModels.Base;
using TinkoffTradeSimulator.Views.Windows;

namespace TinkoffTradeSimulator.ViewModels
{
    internal class MainWindowViewModel : BaseViewModel
    {
        #region Приватные поля
        private ObservableCollection<TickerInfo>? _tickerInfoList;
        private string _title;
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


        // Конструктор
        public MainWindowViewModel()
        {
            LoadData();
            ChartWindowViewModel chartWindow = new ChartWindowViewModel();

            OpenChartWindow();
        }

        #region Методы

        // Загружаю актуальные данные из Tinkoff InvestAPI 
        public async Task LoadData()
        {

            // Загрузите данные из Tinkoff API асинхронно
            await Task.Delay(1000); // Пример задержки, замените на реальную загрузку данных

            // Создаю клиента Тинькофф 
            var client = await TinkoffClient.CreateAsync();

            // Получаю все актуальные данные от сервера
            var instruments = await client?.Instruments?.SharesAsync();

            // Инициализирую коллекцию
            TickerInfoList = new ObservableCollection<TickerInfo>();

            // Заполняю данными список который будет источником данных для отображения в главном окне
            foreach (var instrument in instruments.Instruments)
            {
                TickerInfoList.Add(new TickerInfo { Id = instrument.Isin, TickerName = instrument.Ticker });
            }
        }

        // Открываю окно и строю в нём график
        private void OpenChartWindow()
        {
            // Создаем новую ViewModel для окна
            var chartViewModel = new ChartWindowViewModel();

            // Здесь передаем данные для графика в chartViewModel
            

            // Создаем новое окно и передаем ему ViewModel
            var chartWindow = new ChartWindow();

            // Открываем окно
            chartWindow.Show();
        }
        #endregion

        #region Комманды

        #endregion
    }

}
