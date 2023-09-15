using ScottPlot;
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
using Tinkoff.InvestApi;
using TinkoffTradeSimulator.ApiServices;
using TinkoffTradeSimulator.Models;
using TinkoffTradeSimulator.ViewModels;

namespace TinkoffTradeSimulator.Views.Windows
{
    /// <summary>
    /// Логика взаимодействия для ChartWindow.xaml
    /// </summary>
    public partial class ChartWindow : Window
    {

        private ChartWindowViewModel _chartViewModel;

        public ChartWindow()
        {
            InitializeComponent();

            WpfPlot1.MouseWheel += OnMouseWheel;
        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt))
            {
                // Если зажата клавиша ALT, то изменяем интервал свечей
                if (e.Delta > 0)
                {

                    // Приближение (уменьшение интервала)
                    _chartViewModel.DecreaseCandleInterval();                   
                }
                else
                {
                    // Отдаление (увеличение интервала)
                    _chartViewModel.IncreaseCandleInterval();                    
                }

                // Обновляем график с новым интервалом свечей
                WpfPlot1.Refresh();
            }
        }

            private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Создаю экземпляр класса и передаю в конструкторе WpfPlot который создан во View, имя тикера (заголовок окна надо передавать по другому)
            ChartWindowViewModel _chartViewModel = new(WpfPlot1, Title);
        }
    }
}
