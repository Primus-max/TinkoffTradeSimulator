using System;
using System.Threading.Tasks;
using System.Windows;
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
            await _chartViewModel.UpdateData1(ticker);
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
