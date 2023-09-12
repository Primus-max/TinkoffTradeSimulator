﻿using ScottPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TinkoffTradeSimulator.Models;

namespace TinkoffTradeSimulator.Views.Windows
{
    /// <summary>
    /// Логика взаимодействия для ChartWindow.xaml
    /// </summary>
    public partial class ChartWindow : Window
    {
        public ChartWindow()
        {
            InitializeComponent();


            var plt = new Plot(600, 400);

            // Each candle is represented by a single OHLC object.
            OHLC price = new(
                open: 100,
                high: 120,
                low: 80,
                close: 105,
                timeStart: new DateTime(2021, 09, 24),
                timeSpan: TimeSpan.FromDays(1));

            // Users could be build their own array of OHLCs, or lean on 
            // the sample data generator to simulate price data over time.
            OHLC[] prices = DataGen.RandomStockPrices(new Random(0), 60);

            // Add a financial chart to the plot using an array of OHLC objects
            WpfPlot1.Plot.AddCandlesticks(prices);

            WpfPlot1.Refresh();
           // WpfPlot1.Render();
        }

       
    }
}
