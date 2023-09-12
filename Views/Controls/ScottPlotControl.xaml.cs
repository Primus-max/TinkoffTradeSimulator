using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using TinkoffTradeSimulator.Models;

namespace TinkoffTradeSimulator.Views.Controls
{
    public partial class ScottPlotControl : UserControl
    {
        public static readonly DependencyProperty CandlestickDataProperty =
            DependencyProperty.Register("CandlestickData", typeof(ObservableCollection<Candlestick>), typeof(ScottPlotControl), new PropertyMetadata(null));

        public ObservableCollection<Candlestick> CandlestickData
        {
            get { return (ObservableCollection<Candlestick>)GetValue(CandlestickDataProperty); }
            set { SetValue(CandlestickDataProperty, value); }
        }

        public ScottPlotControl()
        {
            InitializeComponent();
        }
    }
}
