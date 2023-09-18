using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinkoffTradeSimulator.Data
{
    internal class DbManager
    {
        private  AppContext _db = null!;

        public DbManager()
        {
           _db = new AppContext();
        }

        public  AppContext InitializeDB()
        {
            // Экземпляр базы данных
            _db = new AppContext();
            // гарантируем, что база данных создана
            _db.Database.EnsureCreated();
            // загружаем данные из БД
            _db.TradeRecordsInfo.Load();

            return _db;
        }
    }
}
