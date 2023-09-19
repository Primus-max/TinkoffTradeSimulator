using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using TinkoffTradeSimulator.ViewModels;
using ScottPlot;

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

            // Отключаю события скролла от ScottPlot
            WpfPlot1.Configuration.ScrollWheelZoom = false;

            // Добавляю своё событие скролла
            WpfPlot1.MouseWheel += OnMouseWheel;
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
            WpfPlot1.Refresh();

            // Отменим дальнейшую обработку события, чтобы избежать дополнительной прокрутки
            e.Handled = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Создаю экземпляр класса и передаю в конструкторе WpfPlot который создан во View,
            // имя тикера (заголовок окна надо передавать по другому)
            _chartViewModel = new ChartWindowViewModel(WpfPlot1, Title);
        }
    }
}
