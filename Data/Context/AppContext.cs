using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinkoffTradeSimulator.Models;

namespace TinkoffTradeSimulator
{
    internal class AppContext : DbContext
    {
        public DbSet<TradeRecordInfo> TradeRecordsInfo{ get; set; } = null!;
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=flats.db");
        }
    }
}
