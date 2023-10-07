using Microsoft.EntityFrameworkCore;
using TinkoffTradeSimulator.Models;

namespace TinkoffTradeSimulator
{
    internal class AppContext : DbContext
    {
        public DbSet<TradeRecordInfo> TradeRecordsInfo { get; set; } = null!;
        public DbSet<HistoricalTradeRecordInfo> HistoricalTradeRecordsInfo { get; set; } = null!;
        public DbSet<FavoriteTicker> FavoriteTickers { get; set; } = null!;
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=trading.db");
        }
    }
}
