using System;
using System.Threading.Tasks;
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

            // Вызываю метод загрузки исторических данных свечей для отображения во View
            Loaded += ChartWindow_Loaded;

            MouseWheel += MainWindow_MouseWheel;
        }

        private void MainWindow_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                // Прокрутка колесика мыши вперед
                // Вызовите метод для обработки прокрутки вперед
                _chartViewModel.IncreaseCandleHistorical();
            }
            else
            {
                // Прокрутка колесика мыши назад
                // Вызовите метод для обработки прокрутки назад
                _chartViewModel.DecreaseCandleIHistorical();
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
            // Вызываем метод UpdateData, когда требуется обновление данных
            await _chartViewModel.SetAndUpdateCandlesChartWindow(ticker: ticker);
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
    }
}
