using System.Collections.Generic;
using System.Text.Json;

namespace SOD.StockMarket.Implementation.Trade
{
    internal sealed class TradeSaveData
    {
        public Dictionary<int, decimal> PlayerStocks { get; set; }
        public List<TradeOrder> PlayerTradeOrders { get; set; }
        public List<HistoricalPortfolio> HistoricalPortfolio { get; set; }
        public List<TradeHistory> TradeHistory { get; set; }
        public decimal AvailableFunds { get; set; }

        internal string ToJson()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = false });
        }

        internal static TradeSaveData FromJson(string json)
        {
            return JsonSerializer.Deserialize<TradeSaveData>(json);
        }
    }
}
