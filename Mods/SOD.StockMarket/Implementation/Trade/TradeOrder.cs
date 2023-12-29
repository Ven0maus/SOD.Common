namespace SOD.StockMarket.Implementation.Trade
{
    internal class TradeOrder
    {
        internal int StockId { get; }
        internal decimal Price { get; }
        internal int Amount { get; }
        internal OrderType OrderType { get; }
        internal bool Completed { get; private set; }

        internal TradeOrder(OrderType orderType, int stockId, decimal price, int amount)
        {
            StockId = stockId;
            Price = price;
            Amount = amount;
            OrderType = orderType;
            Completed = false;
        }

        internal void Complete()
        {
            Completed = true;
        }
    }

    internal enum OrderType
    {
        Buy,
        Sell
    }
}
