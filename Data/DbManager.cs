using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinkoffTradeSimulator.Models;

namespace TinkoffTradeSimulator.Data
{
    internal class DbManager
    {
        private static  AppContext _db = null!;

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
            _db.HIstoricalTradeRecordsInfo.Load();

            return _db;
        }
        
        public static void SaveHistoricalData(TradeRecordInfo recordInfo)
        {
            _db.HIstoricalTradeRecordsInfo.Add(recordInfo);
            _db.SaveChanges();
        }

        public static void SaveTradingData(TradeRecordInfo recordInfo) 
        {
            _db.TradeRecordsInfo.Add(recordInfo);
            _db.SaveChanges();
        }

        public static void Update(TradeRecordInfo recordInfo) 
        { 
        }
        public static void Delete(TradeRecordInfo recordInfo)
        {
            var existingRecord = _db?.TradeRecordsInfo.FirstOrDefault(r => r.Id == recordInfo.Id);

            if (existingRecord != null)
            {
                _db?.TradeRecordsInfo.Remove(existingRecord);
                _db?.SaveChanges();
            }
        }
    }
}
