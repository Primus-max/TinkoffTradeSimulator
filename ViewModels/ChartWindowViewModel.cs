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
        private ObservableCollection<Candlestick> _candlestickData;
        #endregion

        #region Публичные свойства
        public string Title
        {
            get => _title;
            set => Set(ref _title, value);
        }

        // Публичное свойство для хранения данных свечей
        public ObservableCollection<Candlestick> CandlestickData 
        { 
            get => _candlestickData; 
            set => Set(ref _candlestickData, value); 
        }

        #endregion
        public ChartWindowViewModel()
        {
            // Заголовок окна
            Title = "Тестовое окно с графиками";

            // Получаю данные для отображения свечей
            CandlestickData = new ObservableCollection<Candlestick>(GenerateTestData());

            //ChartWindow chartWindow = new ChartWindow();
            //chartWindow.Show();
        }

        #region Методы
        private List<Candlestick> GenerateTestData()
        {
            List<Candlestick> data = new List<Candlestick>();

            // Генерируйте свои свечи или загружайте реальные данные сюда
            // Пример заполнения случайными данными:
            Random rand = new Random();
            DateTime currentDate = DateTime.Now.Date;
            double previousClose = 100.0;

            for (int i = 0; i < 100; i++)
            {
                double open = previousClose;
                double close = open + rand.NextDouble() * 10 - 5; // Случайное изменение цены
                double high = Math.Max(open, close) + rand.NextDouble() * 5;
                double low = Math.Min(open, close) - rand.NextDouble() * 5;

                data.Add(new Candlestick
                {
                    Date = currentDate,
                    Open = open,
                    High = high,
                    Low = low,
                    Close = close
                });

                previousClose = close;
                currentDate = currentDate.AddDays(1); // Переход к следующей дате
            }

            return data;
        }
        #endregion
    }
}
