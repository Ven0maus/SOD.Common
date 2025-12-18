using SOD.Common;
using SOD.Common.Helpers;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;

namespace SOD.LifeAndLiving.Content.SocialRelation
{
    /// <summary>
    /// Object that keeps track of all the interests of the player.
    /// </summary>
    public class PlayerInterests
    {
        private static PlayerInterests _instance;
        /// <summary>
        /// The instance accessor for the RelationManager class
        /// </summary>
        public static PlayerInterests Instance => _instance ??= new PlayerInterests();

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
                if (!fromPurchases.ContainsKey(key))
                    fromPurchases.Add(key, 1);
                else
                    fromPurchases[key] += 1;
            }
        }

        internal void Load(SaveGameArgs saveGameArgs)
        {
            var hash = Lib.SaveGame.GetUniqueString(saveGameArgs.FilePath);
#pragma warning disable CS0618 // Type or member is obsolete
            var playerInterestsPath = Lib.SaveGame.GetSavestoreDirectoryPath(Assembly.GetExecutingAssembly(), $"PlayerInterests_{hash}.json");
#pragma warning restore CS0618 // Type or member is obsolete
            var newPath = Lib.SaveGame.MigrateOldSaveStructure(playerInterestsPath, saveGameArgs, "sod_lifeandliving_PlayerInterests.json");
            
            // Player interests loading
            if (File.Exists(newPath))
            {
                var playerInterestsJson = File.ReadAllText(newPath);
                var pInterest = JsonSerializer.Deserialize<PlayerInterests>(playerInterestsJson);

                // Set properties
                ItemsPurchased = pInterest.ItemsPurchased;
                PurchasedItemsFrom = pInterest.PurchasedItemsFrom;
            }
        }

        internal void Save(SaveGameArgs saveGameArgs)
        {
            var hash = Lib.SaveGame.GetUniqueString(saveGameArgs.FilePath);
#pragma warning disable CS0618 // Type or member is obsolete
            var playerInterestsPath = Lib.SaveGame.GetSavestoreDirectoryPath(Assembly.GetExecutingAssembly(), $"PlayerInterests_{hash}.json");
#pragma warning restore CS0618 // Type or member is obsolete
            var newPath = Lib.SaveGame.MigrateOldSaveStructure(playerInterestsPath, saveGameArgs, "sod_lifeandliving_PlayerInterests.json");

            if (!ContainsContent)
            {
                if (File.Exists(newPath))
                    File.Delete(newPath);
            }
            else
            {
                var playerInterestsJson = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = false });
                File.WriteAllText(newPath, playerInterestsJson);
            }
        }

        internal void Delete(SaveGameArgs saveGameArgs)
        {
            // Still support migration, deletion handled by sod.common
            var hash = Lib.SaveGame.GetUniqueString(saveGameArgs.FilePath);
#pragma warning disable CS0618 // Type or member is obsolete
            var playerInterestsPath = Lib.SaveGame.GetSavestoreDirectoryPath(Assembly.GetExecutingAssembly(), $"PlayerInterests_{hash}.json");
#pragma warning restore CS0618 // Type or member is obsolete
            _ = Lib.SaveGame.MigrateOldSaveStructure(playerInterestsPath, saveGameArgs, "sod_lifeandliving_PlayerInterests.json");
        }
    }
}
