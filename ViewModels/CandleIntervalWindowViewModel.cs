using System.Collections.ObjectModel;
using TinkoffTradeSimulator.Models;
using TinkoffTradeSimulator.ViewModels.Base;

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

        }
        #region Методы

        #endregion

        #region Комманды

        #endregion

    }
}
