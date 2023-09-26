using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.Wpf;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
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


            //// Отключаю события скролла от ScottPlot
            //WpfPlot1.Configuration.ScrollWheelZoom = false;

            //// Добавляю своё событие скролла
            //WpfPlot1.MouseWheel += OnMouseWheel;

            PlotModel plot =  InitializePlotModel();

           
            var asdf = "";
        }


        public static PlotModel InitializePlotModel()
        {
            var model = new PlotModel { Title = "" };

            var axisColor = OxyColor.FromRgb(227, 227, 227);
            var gridLineColor = OxyColor.FromRgb(81, 81, 81);

            //title
            model.TitleColor = OxyColors.DodgerBlue;
            model.TitleFont = "Segoe UI Semibold";

            //Legend
            //model.LegendFont = "Segoe UI Semibold";
            //model.LegendTextColor = axisColor;
            //model.LegendBorder = gridLineColor;

            //plot border
            model.PlotAreaBorderColor = axisColor;

            //axes
            model.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Right,
                TextColor = axisColor,
                AxislineColor = axisColor,
                TicklineColor = axisColor,
                MajorGridlineColor = gridLineColor,
                MajorGridlineStyle = LineStyle.Dash,
                MinorGridlineColor = gridLineColor,
                MinorGridlineStyle = LineStyle.Dash,
                Font = "Segoe UI Semibold",
                AbsoluteMinimum = 0
            });

            model.Axes.Add(new DateTimeAxis
            {
                Position = AxisPosition.Bottom,
                TextColor = axisColor,
                AxislineColor = axisColor,
                TicklineColor = axisColor,
                MajorGridlineColor = gridLineColor,
                MinorTickSize = 0,
                Font = "Segoe UI Semibold"
            });

            //prices
            model.Series.Add(new CandleStickSeries
            {
                CandleWidth = .4,
                Color = OxyColors.DimGray,
                IncreasingColor = OxyColor.FromRgb(0, 192, 0),
                DecreasingColor = OxyColor.FromRgb(255, 0, 0),
                StrokeThickness = 1
            });

            //buys and sells (markers)
            model.Series.Add(new ScatterSeries
            {
                Title = "Buys",
                MarkerFill = OxyColors.DodgerBlue,
                MarkerType = MarkerType.Triangle,
                MarkerSize = 3
            });

            model.Series.Add(new ScatterSeries
            {
                Title = "Sells",
                MarkerFill = OxyColor.FromRgb(222, 222, 222),
                MarkerType = MarkerType.Circle,
                MarkerSize = 3
            });

            return model;
        }


        // Событие сролла мышки для масштабирования таймфрейма свечи
        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {

            if (e.Delta > 0)
            {
                // Скролл вперёд (увеличение интервала)
                _chartViewModel.IncreaseCandleHistorical();
            }
            else
            {
                // Скролл назад (уменьшение интервала)
                _chartViewModel.DecreaseCandleIHistorical();
            }

            // Обновляем график с новым интервалом свечей
            // WpfPlot1.Refresh();

            // Отменим дальнейшую обработку события, чтобы избежать дополнительной прокрутки
            e.Handled = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Создаю экземпляр класса и передаю в конструкторе WpfPlot который создан во View,
            // имя тикера (заголовок окна надо передавать по другому)
            //_chartViewModel = new ChartWindowViewModel(WpfPlot1, Title);
        }
    }
}
