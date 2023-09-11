using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinkoffTradeSimulator.ApiServices;
using TinkoffTradeSimulator.Models;
using TinkoffTradeSimulator.ViewModels.Base;

namespace TinkoffTradeSimulator.ViewModels
{
    internal class MainWindowViewModel: BaseViewModel
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
        public  MainWindowViewModel()
        {
             LoadData();
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

        
        #endregion
    }
    
}
