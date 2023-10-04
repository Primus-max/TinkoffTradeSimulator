using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using TinkoffTradeSimulator.Services;
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
            _chartViewModel.PlotModel = CandlestickPlot;


            EventAggregator.UpdateDataRequested += EventAggregator_UpdateDataRequested;

            //_chartViewModel.UpdateData(1);
        }

     
        //public void InitializePlot()
        //{
        //    var plotModel = new PlotModel { Title = "Candlestick Chart" };
        //    var candlestickSeries = new CandleStickSeries
        //    {
        //        Title = "Candlesticks",
        //        TrackerFormatString = "Date: {2:yyyy-MM-dd}\nOpen: {5}\nHigh: {3}\nLow: {4}\nClose: {6}"
        //    };
        //    candlestickSeries.Items.AddRange(_chartViewModel.CandlestickData.Select(data => new HighLowItem(
        //        DateTimeAxis.ToDouble(data.Date),
        //        data.High,
        //        data.Low,
        //        data.Open,
        //        data.Close
        //    )));
        //    plotModel.Series.Add(candlestickSeries);
        //    CandlestickPlot.Model = plotModel;
        //}

        //private void Button_Click(object sender, RoutedEventArgs e)
        //{            
        //    _chartViewModel.UpdateData(1);
        //}

        private async void EventAggregator_UpdateDataRequested()
        {
            string? ticker = Title;
            // Вызываем метод UpdateData, когда требуется обновление данных
            await _chartViewModel.UpdateData1(ticker);
        }

        private void ChartWindow_Closed(object sender, EventArgs e)
        {
            EventAggregator.UpdateDataRequested -= EventAggregator_UpdateDataRequested;
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
