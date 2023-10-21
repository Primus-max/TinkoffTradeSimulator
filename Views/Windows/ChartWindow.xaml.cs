using DromAutoTrader.Views;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TinkoffTradeSimulator.Models;
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
            EventAggregator.UpdateTickerInfo += EventAggregator_UpdateTickerInfo;

            // Вызываю метод загрузки исторических данных свечей для отображения во View
            Loaded += ChartWindow_Loaded;
            MouseWheel += MainWindow_MouseWheel;                    
        }

        // Метод подписчика на событие об изменении информации о тикере
        private void EventAggregator_UpdateTickerInfo(TickerInfo tickerInfo)
        {
            TickerPriceTextBlock.Text = tickerInfo?.Price != null ? $"{tickerInfo.Price} ₽" : string.Empty;
            MaxPriceTextBlock.Text = tickerInfo?.MaxPrice != null ? $"{tickerInfo.MaxPrice} ₽" : string.Empty;
            MinPriceTextBlock.Text = tickerInfo?.MinPrice != null ? $"{tickerInfo.MinPrice} ₽" : string.Empty;

            // Передаю обратно информацию о тикере во ViewModel
            LocatorService.Current.TickerInfo = tickerInfo;
        }

        private void MainWindow_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                // Прокрутка колесика мыши вперед
                _chartViewModel.DecreaseCandleIHistorical();

            }
            else
            {
                // Прокрутка колесика мыши назад                
                _chartViewModel.IncreaseCandleHistorical();
            }

            e.Handled = true; // Предотвратите дальнейшее распространение события
        }

        #region Отображение свечей
        private async void ChartWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await SetCandlesTOView();
        }

        public async Task SetCandlesTOView()
        {
            string? ticker = Title;           

            // Получаю тикеры в локальное хранилище
            await _chartViewModel.GetLastCandlesForLocalSotarageAsync(ticker);
        }

        private async void EventAggregator_UpdateDataRequested()
        {
            // Отображаю свечи
            await SetCandlesTOView();
        }
        private void ChartWindow_Closed(object sender, EventArgs e)
        {
            EventAggregator.UpdateDataRequested -= EventAggregator_UpdateDataRequested;
        }
        #endregion

        //private void Window_Loaded(object sender, RoutedEventArgs e)
        //{
        //    TimeFrameButton.Content = "";
        //}
    }
}
