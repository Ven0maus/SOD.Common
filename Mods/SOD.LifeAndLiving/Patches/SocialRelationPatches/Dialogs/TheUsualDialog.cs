using SOD.Common;
using SOD.Common.Helpers.DialogObjects;
using SOD.LifeAndLiving.Relations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SOD.LifeAndLiving.Patches.SocialRelationPatches.DialogLogic
{
    internal class TheUsualDialog : IDialogLogic
    {
        /// <summary>
        /// Add's several dialog options regarding purchasing items often at places.
        /// </summary>
        internal static void Register()
        {
            _ = Lib.Dialog.Builder($"{Plugin.PLUGIN_GUID.GetHashCode()}_TheUsualPurchase")
                .SetText("The usual please.")
                .AddResponse("Ahh yes, coming right up sir!", isSuccesful: true)
                .AddResponse("Looks like you can't afford it today.", isSuccesful: false)
                .ModifyDialogOptions((a) =>
                {
                    a.useSuccessTest = true;
                    a.ranking = 100;
                })
                .SetDialogLogic(new TheUsualDialog())
                .CreateAndRegister();
        }

        private Item _item = null;

        public bool IsDialogShown(DialogPreset preset, Citizen saysTo, SideJob jobRef)
        {
            // Check if this citizen has sold items to us before, and if one of them was sold more or equal than 5 times
            if (saysTo == null || !saysTo.isAtWork) return false;
            if (!RelationManager.Instance.PlayerInterests.PurchasedItemsFrom.TryGetValue(saysTo.humanID, out var items))
                return false;
            return items.Any(a => a.Value >= 5);
        }

        public void OnDialogExecute(DialogController instance, Citizen saysTo, Interactable saysToInteractable, NewNode where, Actor saidBy, bool success, NewRoom roomRef, SideJob jobRef)
        {
            // Collect most purchased item
            if (_item != null)
            {
                // TODO: Give item to the player, reduce money if required

                Plugin.Log.LogInfo("Received item");

                // Reset to null
                _item = null;
            }
        }

        public DialogController.ForceSuccess ShouldDialogSucceedOverride(DialogController instance, EvidenceWitness.DialogOption dialog, Citizen saysTo, NewNode where, Actor saidBy)
        {
            if (!saysTo)
                return DialogController.ForceSuccess.none;

            _item = CollectMostPurchasedItem(RelationManager.Instance.PlayerInterests.PurchasedItemsFrom[saysTo.humanID]);
            if (_item == null)
            {
                Plugin.Log.LogInfo("Could not find item, this should not have happened.");
                return DialogController.ForceSuccess.fail;
            }

            // TODO: Check if we have enough cash
            // Or succes if its on the house

            //if (notEnoughCash)
            //  return DialogController.ForceSuccess.fail;

            return DialogController.ForceSuccess.none;
        }

        private static Item CollectMostPurchasedItem(Dictionary<int, int> items)
        {
            var mostPurchasedItemCode = items
                .Where(a => a.Value >= 5)
                .OrderByDescending(a => a.Value)
                .First()
                .Key;

            // Find the item name
            string itemName = null;
            foreach (var data in RelationManager.Instance.PlayerInterests.ItemsPurchased)
            {
                var companyId = data.Key;
                foreach (var item in data.Value)
                {
                    var itemCode = HashCode.Combine(companyId, item.Key);
                    if (mostPurchasedItemCode == itemCode)
                    {
                        itemName = item.Key;
                        break;
                    }
                }
                if (itemName != null)
                    break;
            }

            // TODO: See if we can find interactable / preset
            // TODO: See if we can find the price of this item

            return itemName == null ? null : new Item(itemName, null, 0);
        }

        class Item
        {
            public string Name { get; }
            public Interactable Interactable { get; }
            public int Price { get; }

            public Item(string itemName, Interactable interactable, int price)
            {
                Name = itemName;
                Interactable = interactable;
                Price = price;
            }
        }
    }
}
