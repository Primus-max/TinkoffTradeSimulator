using AppContext = TinkoffTradeSimulator.AppContext;

namespace DromAutoTrader.Data
{
    internal class AppContextFactory
    {
        private static AppContext? _instance;

        public static AppContext GetInstance()
        {
            if (_instance == null)
            {
                _instance = new AppContext();
                _instance.Database.EnsureCreated(); // Проверка и создание базы данных при необходимости
            }

            return _instance;
        }
    }

}
