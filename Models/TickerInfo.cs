namespace TinkoffTradeSimulator.Models
{
    public class TickerInfo
    {
        public string? Id { get; set; }    
        public string? TickerName { get; set; }
        public string? Open { get; set; } = string.Empty;
        public string? Close { get; set; } = string.Empty;         
        public string? Price { get; set; } = string.Empty;
        public string? MaxPrice { get; set; } = string.Empty;
        public string? MinPrice { get; set;} = string.Empty;
    }

}
