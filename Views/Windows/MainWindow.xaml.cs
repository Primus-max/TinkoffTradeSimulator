using System.Windows;
using Tinkoff.InvestApi.V1;
using TinkoffTradeSimulator.ApiServices;
using TinkoffTradeSimulator.ApiServices.Tinkoff;

namespace TinkoffTradeSimulator.Views.Windows
{
    /// <summary> Главное окно приложения </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void GetPrice_Click(object sender, RoutedEventArgs e)
        {
           var client = await TinkoffClient.CreateAsync();
            var nstruments = await client?.Instruments?.SharesAsync();


            foreach ( var n in nstruments.Instruments) 
            { 
                var ticker =  n.Ticker;
                var price = n.Nominal;
            }

            Instrument instrument1 = new Instrument();
           
        }
    }
}
