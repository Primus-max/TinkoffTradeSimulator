using System.Windows;
using System.Windows.Controls;
using TinkoffTradeSimulator.Services;
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

        #region ФИЛЬТРЫ
        private void FilterByTickerAll_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Получите текст из TextBox
            string filterText = ((TextBox)sender).Text;

            // Вызываю метод фильрации метод фильтрации
            _mainWindowViewModel.UpdateFilteredTickerInfoList(filterText);
        }

        private void FilterByTickerTradeRecordHistorial_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Получите текст из TextBox
            string filterText = ((TextBox)sender).Text;

            // Вызываю метод фильрации метод фильтрации
            _mainWindowViewModel.UpdateFilterTradeHistoricalInfoListByTicker(filterText);
        }
        #endregion

        private void FilterByTickerTradingRecord_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Получите текст из TextBox
            string filterText = ((TextBox)sender).Text;

            // Вызываю метод фильрации метод фильтрации
            _mainWindowViewModel.UpdateFilterTradingInfoListByTicker(filterText);
        }

        
    }
}
