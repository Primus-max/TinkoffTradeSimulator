using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TinkoffTradeSimulator.Models;
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

        #region По всем тикерам
        private void FilterByTickerAll_TextChanged(object sender, TextChangedEventArgs e)
        {
            var collection = (CollectionViewSource)TickersCollectionDockPanelDataGrid.FindResource("TickersCollection");
            collection.View.Refresh();
        }
        private void TickersCollection_Filter(object sender, System.Windows.Data.FilterEventArgs e)
        {
            if ((e.Item is not TickerInfo ticker)) return;
            if (string.IsNullOrEmpty(ticker.TickerName)) return;

            string filterText = TextSearchParameterTextBox.Text;
            if (string.IsNullOrEmpty(filterText)) return;

            if (ticker.TickerName.Contains(filterText, StringComparison.OrdinalIgnoreCase)) return;

            e.Accepted = false;
        }
        #endregion

        private void FilterByTickerTradeRecordHistorial_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Получите текст из TextBox
            string filterText = ((TextBox)sender).Text;

            // Вызываю метод фильрации
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
