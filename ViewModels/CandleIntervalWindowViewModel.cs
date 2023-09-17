using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using Tinkoff.InvestApi.V1;
using Tinkoff.Trading.OpenApi.Models;
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
        #endregion

        #region Публичные свойства
        public ObservableCollection<CandleTimeFrameButton> CandleTimeFrameButtons
        {
            get => _candleTimeFrameButtons;
            set => Set(ref _candleTimeFrameButtons, value);
        }
        #endregion

        public CandleIntervalWindowViewModel()
        {
            #region Инициализация команд
            CloseCandleIntervalWindowCommand = new LambdaCommand(OnCloseleIntervalWindowCommandExecuted, CanCloseCandleIntervalWindowCommandExecute);
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
        #endregion



        #region Методы
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

                    // Уберите символ "_"
                    name = name.Replace("_", "");

                    // Создайте TimeSpan на основе числового значения и единицы измерения
                    var timeSpan = TimeSpan.FromMinutes((int)interval);

                    // Создайте и добавьте кнопку во временную коллекцию
                    tempCollection.Add(new CandleTimeFrameButton { Name = name, Time = timeSpan });
                }
            }

            // Присвойте временную коллекцию свойству CandleTimeFrameButtons
            CandleTimeFrameButtons = tempCollection;
        }

        // Метод закрытия окна с выбором таймфреймов
        private void CloseCandleIntervalWindow()
        {
            // Здесь выполняется закрытие окна CandleIntervalWindow
            Application.Current.Windows.OfType<CandleIntervalWindow>().FirstOrDefault()?.Close();
        
        }

        #endregion

    }
}
