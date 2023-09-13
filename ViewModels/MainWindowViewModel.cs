using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Input;
using Tinkoff.InvestApi;
using Tinkoff.InvestApi.V1;
using TinkoffTradeSimulator.ApiServices;
using TinkoffTradeSimulator.ApiServices.Tinkoff;
using TinkoffTradeSimulator.Infrastacture.Commands;
using TinkoffTradeSimulator.Models;
using TinkoffTradeSimulator.ViewModels.Base;
using TinkoffTradeSimulator.Views.Windows;

namespace TinkoffTradeSimulator.ViewModels
{
    internal class MainWindowViewModel : BaseViewModel
    {
        #region Приватные поля
        private  InvestApiClient? _client = null;
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

            

            LoadData();
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
        private async void OpenChartWindow(string tickerName)
        {
            // Создаю новую ViewModel для окна
            var chartViewModel = new ChartWindowViewModel();

            // Устанавливаю значение Title через свойство
            chartViewModel.Title = tickerName;            

            // Создаем новое окно и передаем ему ViewModel
            var chartWindow = new ChartWindow();
            // Устанавливаю контекст даннх для окна (странно, но это так же делаю в самом окне)
            chartWindow.DataContext = chartViewModel;

            // Открываем окно
            chartWindow.Show();
        }
        #endregion
    }

}
