using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Documents;
using Tinkoff.InvestApi.V1;
using TinkoffTradeSimulator.ApiServices;
using TinkoffTradeSimulator.ApiServices.Tinkoff;
using TinkoffTradeSimulator.Models;
using TinkoffTradeSimulator.ViewModels;
using static System.Net.Mime.MediaTypeNames;

namespace TinkoffTradeSimulator.Views.Windows
{
    /// <summary> Главное окно приложения </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Loaded += MainWindow_Loaded;
            //GetPrice();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            MainWindowViewModel viewModel = new();
            //viewModel.();
        }

        //private async void GetPrice()
        //{
        //   var client = await TinkoffClient.CreateAsync();
        //    var nstruments = await client?.Instruments?.SharesAsync();

            

        //    List<TickerInfo> tickers = new List<TickerInfo>();

        //    foreach ( var n in nstruments.Instruments) 
        //    {
        //        TickerInfo tickerInfo = new TickerInfo
        //        {
        //            Id = n.Isin,
        //            TickerName = n.Name
        //        };

        //        tickers.Add(tickerInfo);
        //    }

            

        //    Instrument instrument1 = new Instrument();
           
        //}

        private void ListView_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        }
    }
}
