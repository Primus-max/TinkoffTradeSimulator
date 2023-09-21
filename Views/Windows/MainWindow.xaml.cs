using System.Windows;
using System.Windows.Controls;
using TinkoffTradeSimulator.ViewModels;

namespace TinkoffTradeSimulator.Views.Windows
{
    /// <summary> Главное окно приложения </summary>
    public partial class MainWindow : Window
    {
        MainWindowViewModel _mainWindowViewModel = null!;
        public MainWindow()
        {
            InitializeComponent();
            _mainWindowViewModel = new MainWindowViewModel();
            DataContext = _mainWindowViewModel;                
        }

        private void FilterByTickerAll_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Получите текст из TextBox
            string filterText = ((TextBox)sender).Text;

            // Вызовите метод фильтрации
            _mainWindowViewModel.UpdateFilteredTickerInfoList(filterText);
        }

        private void FilterByTickerTradeRecordHistorial_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Получите текст из TextBox
            string filterText = ((TextBox)sender).Text;

            // Вызываем метод фильтрации для TradeHistoricalInfoList в вашей ViewModel
            _mainWindowViewModel.FilterTradeHistoricalInfoListByTicker(filterText);

            // Вызываем метод фильтрации для TradingInfoList в вашей ViewModel
            //ViewModel.FilterTradingInfoListByTicker(filterText);
        }
    }
}
