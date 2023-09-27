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


        // Открытие окна с выбором таймфрема
        //private void UpdateButtonClick(object sender, RoutedEventArgs e)
        //{
        //    var intervalSelectionWindow = new IntervalSelectionWindow();
        //    intervalSelectionWindow.Owner = this; // Устанавливаем текущее окно как владельца для модального диалогового окна
        //    intervalSelectionWindow.ShowDialog();

        //    // После закрытия окна выбора интервала, обновляем данные и график
        //    int selectedInterval = intervalSelectionWindow.SelectedInterval;
        //    viewModel.UpdateData(selectedInterval);
        //    InitializePlot();
        //}
    }  
}
