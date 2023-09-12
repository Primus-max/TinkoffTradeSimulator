using ScottPlot;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinkoffTradeSimulator.Models;
using TinkoffTradeSimulator.ViewModels.Base;
using TinkoffTradeSimulator.Views.Windows;

namespace TinkoffTradeSimulator.ViewModels
{
    class ChartWindowViewModel: BaseViewModel
    {
        
        #region Приватные свойства
        private string _title;

        // Приватное свойство для хранения данных свечей
        private ObservableCollection<OHLC> _candlestickData;
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

        #endregion
        public ChartWindowViewModel()
        {
            // Заголовок окна
            Title = "Тестовое окно с графиками";

            CandlestickData = new ObservableCollection<OHLC>();

            // Создаем тестовые данные для свечей
            OHLC[] testData = new OHLC[]
            {
        new OHLC(100, 120, 80, 105, new DateTime(1985, 09, 24), TimeSpan.FromDays(1)),
                // Добавьте еще свои тестовые данные здесь...
            };

            // Заполняем коллекцию свечей
            foreach (var dataPoint in testData)
            {
                CandlestickData.Add(dataPoint);
            }
        }

        #region Методы
        // Метод для генерации случайных данных OHLC
       

        #endregion
    }
}
