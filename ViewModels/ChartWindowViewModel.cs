using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinkoffTradeSimulator.ViewModels.Base;
using TinkoffTradeSimulator.Views.Windows;

namespace TinkoffTradeSimulator.ViewModels
{
    class ChartWindowViewModel: BaseViewModel
    {
        
        #region Приватные свойства
        private string _title;
        #endregion

        #region Публичные свойства
        public string Title
        {
            get => _title;
            set => Set(ref _title, value);
        }
        #endregion
        public ChartWindowViewModel()
        {
            Title = "Тестовое окно с графиками";
            //ChartWindow chartWindow = new ChartWindow();
            //chartWindow.Show();
        }   
    }
}
