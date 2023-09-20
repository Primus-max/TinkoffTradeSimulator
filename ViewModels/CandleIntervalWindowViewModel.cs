using ScottPlot;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Tinkoff.InvestApi.V1;
using TinkoffTradeSimulator.ApiServices.Tinkoff;
using TinkoffTradeSimulator.Infrastacture.Commands;
using TinkoffTradeSimulator.Models;
using TinkoffTradeSimulator.ViewModels.Base;
using TinkoffTradeSimulator.Views.Windows;

namespace TinkoffTradeSimulator.ViewModels
{
    /// <summary>
    /// Модель представления для окна с кнопками выбора таймфремов Тинькофф
    /// </summary>
    internal class CandleIntervalWindowViewModel : BaseViewModel
    {
        #region Приватные свойства        
        private ObservableCollection<CandleTimeFrameButton> _candleTimeFrameButtons = null;
        private CandleTimeFrameButton _selectedTimeFrame = null!;
        private EventAggregator _eventAggregator = null!;
        #endregion

        #region Публичные свойства
        public ObservableCollection<CandleTimeFrameButton> CandleTimeFrameButtons
        {
            get => _candleTimeFrameButtons;
            set => Set(ref _candleTimeFrameButtons, value);
        }

        public CandleTimeFrameButton SelectedTimeFrame
        {
            get => _selectedTimeFrame;
            set => Set(ref _selectedTimeFrame, value);
        }

        #endregion

        public CandleIntervalWindowViewModel()
        {

           // EventAggregator.CandleIntervalSelected += OnCandleIntervalSelected;

            #region Инициализация команд
            CloseCandleIntervalWindowCommand = new LambdaCommand(OnCloseleIntervalWindowCommandExecuted, CanCloseCandleIntervalWindowCommandExecute);

            SelectHistoricalCandleIntervalCommand = new LambdaCommand(OnSelectHistoricalCandleIntervalCommandExecuted, CanSelectHistoricalCandleIntervalCommandExecute);
            #endregion

            FillCandleTimeFrameButtons();
        }

       

        #region Команды
        public ICommand? CloseCandleIntervalWindowCommand { get; } = null;

        private bool CanCloseCandleIntervalWindowCommandExecute(object p) => true;

        private void OnCloseleIntervalWindowCommandExecuted(object sender)
        {
            // Закрываю окно с выбором таймфрема для свечи
            CloseCandleIntervalWindow();
        }

        public ICommand? SelectHistoricalCandleIntervalCommand { get; } = null;

        private bool CanSelectHistoricalCandleIntervalCommandExecute(object p) => true;

        private void OnSelectHistoricalCandleIntervalCommandExecuted(object sender)
        {
            // Обновляю данные иакрываю окно с выбором таймфрема для свечи
            HandleTimeFrameButtonClicked((CandleTimeFrameButton)sender);
        }
        #endregion

        #region Методы
        // Обновляю информацию по тикеру на основе переданного таймфрейма
        public async Task HandleTimeFrameButtonClicked(CandleTimeFrameButton selectedButton)
        {
            SelectedTimeFrame = selectedButton;

            // Публикация события об изменении таймфрема из которого берём имя для кнопки
            EventAggregator.PublishCandleIntervalSelected(SelectedTimeFrame);

            if (SelectedTimeFrame != null)
            {
                // Преобразование Name из SelectedTimeFrame в CandleInterval
                if (Enum.TryParse(SelectedTimeFrame.Name, out CandleInterval candleInterval))
                {
                    // Закрываю окно после выбора таймфрейма
                    CloseCandleIntervalWindow();

                    ChartWindowViewModel chartWindowViewModel = new();

                    // Получаю обновлённый список свечей
                    OHLC[] pricesArray = await TinkoffTradingPrices.GetCandlesData(candleInterval: candleInterval);

                    // ОБновляю view
                    chartWindowViewModel.UpdateChartWindow(pricesArray);
                }
                else
                {
                    // Обработка ошибки преобразования
                    // Возможно, вы захотите вывести сообщение об ошибке или выполнить другие действия.
                }
            }
        }

        // Наполняю окно кнопками с таймфреймами
        private void FillCandleTimeFrameButtons()
        {
            // Получите все значения перечисления CadleInterval
            var intervals = Enum.GetValues(typeof(Tinkoff.InvestApi.V1.CandleInterval));

            // Создайте временную коллекцию для хранения кнопок
            var tempCollection = new ObservableCollection<CandleTimeFrameButton>();

            foreach (var interval in intervals)
            {
                if (interval is Tinkoff.InvestApi.V1.CandleInterval cadleInterval)
                {
                    // Имя значения перечисления
                    var name = cadleInterval.ToString();                    

                    // Создайте и добавьте кнопку во временную коллекцию
                    tempCollection.Add(new CandleTimeFrameButton { Name = name });
                }
            }

            // Присвойте временную коллекцию свойству CandleTimeFrameButtons
            CandleTimeFrameButtons = tempCollection;
        }

        // Метод закрытия окна с выбором таймфреймов
        private static void CloseCandleIntervalWindow()
        {
            // Здесь выполняется закрытие окна CandleIntervalWindow
            Application.Current.Windows.OfType<CandleIntervalWindow>().FirstOrDefault()?.Close();

        }

        #endregion

    }
}
