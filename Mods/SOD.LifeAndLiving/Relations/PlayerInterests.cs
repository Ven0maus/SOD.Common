using SOD.Common;
using System;
using System.Collections.Generic;

namespace SOD.LifeAndLiving.Relations
{
    /// <summary>
    /// Object that keeps track of all the interests of the player.
    /// </summary>
    public class PlayerInterests
    {
        /// <summary>
        /// Should this file be serialized?
        /// </summary>
        public bool ContainsContent { get { return ItemsPurchased.Count > 0 || PurchasedItemsFrom.Count > 0; } }

        /// <summary>
        /// Key: CompanyID, Value: (Key: itemName, Value: amountOfTimesBought)
        /// </summary>
        public Dictionary<int, Dictionary<string, int>> ItemsPurchased { get; set; } = new Dictionary<int, Dictionary<string, int>>();
        /// <summary>
        /// Key: HumanID, Value: (Key: (companyId, itemName).HashCode, Value: amountOfTimesBought)
        /// </summary>
        public Dictionary<int, Dictionary<string, int>> PurchasedItemsFrom { get; set; } = new Dictionary<int, Dictionary<string, int>>();

        /// <summary>
        /// Keeps track of what was bought, where and optionally from who.
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="itemName"></param>
        public void RecordPurchasedItem(int companyId, string itemName, Actor from = null)
        {
            if (!ItemsPurchased.TryGetValue(companyId, out Dictionary<string, int> items))
                ItemsPurchased[companyId] = items = new Dictionary<string, int>();

            if (!items.ContainsKey(itemName))
                items.Add(itemName, 1);
            else 
                items[itemName] += 1;

            var human = from as Human;
            if (human != null)
            {
                if (!PurchasedItemsFrom.TryGetValue(human.humanID, out var fromPurchases))
                    PurchasedItemsFrom[human.humanID] = fromPurchases = new Dictionary<string, int>();

                var key = Lib.SaveGame.GetUniqueString(companyId + "_" + itemName);
                Plugin.Log.LogInfo($"Buy id {companyId} | key: {itemName} | itemCode: {key}");
                if (!fromPurchases.ContainsKey(key))
                    fromPurchases.Add(key, 1);
                else
                    fromPurchases[key] += 1;
            }
        }
    }
}
