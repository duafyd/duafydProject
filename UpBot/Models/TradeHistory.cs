namespace UpBot.Models
{
    public class TradeHistory
    {
        public int Id { get; set; }
        public string CoinName { get; set; }
        public decimal BuyPrice { get; set; }
        public decimal SellPrice { get; set; }
        public decimal Quantity { get; set; }
        public decimal ProfitAmount { get; set; }
        public decimal ProfitPercent { get; set; }
        public DateTime TradeDate { get; set; }
    }
}