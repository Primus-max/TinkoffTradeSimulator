using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using TinkoffTradeSimulator.ViewModels;



namespace TinkoffTradeSimulator.Views.Windows
{
    /// <summary>
    /// Логика взаимодействия для ChartWindow.xaml
    /// </summary>
    public partial class ChartWindow : Window
    {

        private ChartWindowViewModel _chartViewModel = null;

            
        public ChartWindow()
        {
            InitializeComponent();


            _chartViewModel = new ChartWindowViewModel();
            DataContext = _chartViewModel;

            InitializePlot();

            //// Отключаю события скролла от ScottPlot
            //WpfPlot1.Configuration.ScrollWheelZoom = false;

            //// Добавляю своё событие скролла
            //WpfPlot1.MouseWheel += OnMouseWheel;

           
        }

        private void InitializePlot()
        {
            var plotModel = new PlotModel { Title = "Candlestick Chart" };
            var candlestickSeries = new CandleStickSeries
            {
                Title = "Candlesticks",
                TrackerFormatString = "Date: {2:yyyy-MM-dd}\nOpen: {5}\nHigh: {3}\nLow: {4}\nClose: {6}"
            };
            candlestickSeries.Items.AddRange(_chartViewModel.CandlestickData.Select(data => new HighLowItem(
                DateTimeAxis.ToDouble(data.Date),
                data.High,
                data.Low,
                data.Open,
                data.Close
            )));
            plotModel.Series.Add(candlestickSeries);
            candlestickPlot.Model = plotModel;
        }       


        // Событие сролла мышки для масштабирования таймфрейма свечи
        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {

            if (e.Delta > 0)
            {
                // Скролл вперёд (увеличение интервала)
                _chartViewModel.IncreaseCandleHistorical();
            }
            else
            {
                // Скролл назад (уменьшение интервала)
                _chartViewModel.DecreaseCandleIHistorical();
            }

            // Обновляем график с новым интервалом свечей
            // WpfPlot1.Refresh();

            // Отменим дальнейшую обработку события, чтобы избежать дополнительной прокрутки
            e.Handled = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Создаю экземпляр класса и передаю в конструкторе WpfPlot который создан во View,
            // имя тикера (заголовок окна надо передавать по другому)
            //_chartViewModel = new ChartWindowViewModel(WpfPlot1, Title);
        }
    }

    public class CandlestickData
    {
        public DateTime Date { get; set; }
        public double High { get; set; }
        public double Low { get; set; }
        public double Open { get; set; }
        public double Close { get; set; }
    }
}
