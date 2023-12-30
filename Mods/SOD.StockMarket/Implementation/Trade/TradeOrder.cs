namespace SOD.StockMarket.Implementation.Trade
{
    internal class TradeOrder
    {
        public int StockId { get; set; }
        public decimal Price { get; set; }
        public int Amount { get; set; }
        public OrderType OrderType { get; set; }
        public bool Completed { get; set; }

        public TradeOrder() { }

        internal TradeOrder(OrderType orderType, int stockId, decimal price, int amount)
        {
            StockId = stockId;
            Price = price;
            Amount = amount;
            OrderType = orderType;
            Completed = false;
        }
    }

    internal enum OrderType
    {
        Buy,
        Sell
    }
}
