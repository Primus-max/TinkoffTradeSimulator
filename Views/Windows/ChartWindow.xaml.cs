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

                    // Приближение (увеличение интервала)
                    _chartViewModel.IncreaseCandleInterval();
                }
                else
                {
                    // Отдаление (уменьшение интервала)
                    _chartViewModel.DecreaseCandleInterval();                    
                }

                // Обновляем график с новым интервалом свечей
                WpfPlot1.Refresh();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Создаю экземпляр класса и передаю в конструкторе WpfPlot который создан во View, имя тикера (заголовок окна надо передавать по другому)
            _chartViewModel = new ChartWindowViewModel(WpfPlot1, Title);
        }
    }
}
