using Newtonsoft.Json;
using System.Collections.Generic;

namespace SOD.StockMarket.Implementation.Trade
{
    internal sealed class TradeSaveData
    {
        public Dictionary<int, int> PlayerStocks { get; set; }
        public List<TradeOrder> PlayerTradeOrders { get; set; }

        internal string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.None);
        }

        internal static TradeSaveData FromJson(string json)
        {
            return JsonConvert.DeserializeObject<TradeSaveData>(json);
        }
    }
}
