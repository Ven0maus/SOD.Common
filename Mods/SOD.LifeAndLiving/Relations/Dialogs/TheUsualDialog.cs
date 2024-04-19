using Rewired;
using SOD.Common;
using SOD.Common.Helpers.DialogObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SOD.LifeAndLiving.Relations.Dialogs
{
    internal class TheUsualDialog : IDialogLogic
    {
        private static readonly Guid _positiveResponse = Guid.NewGuid();
        private static readonly Guid _positiveDiscountResponse = Guid.NewGuid();
        private static readonly Guid _positiveFreeResponse = Guid.NewGuid();

        // TODO: Move to configuration bindings
        private const int DiscountPercentage = 35;
        private const int DiscountChance = 25;
        private const int FreeChance = 10;

        /// <summary>
        /// Add's several dialog options regarding purchasing items often at places.
        /// </summary>
        internal static void Register()
        {
            _ = Lib.Dialog.Builder($"{Plugin.PLUGIN_GUID.GetHashCode()}_TheUsualPurchase")
                .SetText("The usual please.")
                .AddCustomResponse("Ahh yes, coming right up sir!", _positiveResponse)
                .AddCustomResponse("You're lucky, this last one is 50% off the price!", _positiveDiscountResponse)
                .AddCustomResponse("Coming right up, this one's on the house!", _positiveFreeResponse)
                .AddResponse("Looks like you can't afford it today.", isSuccesful: false)
                .ModifyDialogOptions((a) =>
                {
                    a.useSuccessTest = true;
                    a.ranking = int.MaxValue;
                })
                .SetDialogLogic(new TheUsualDialog())
                .CreateAndRegister();
        }

        private Item _item = null;

        public bool IsDialogShown(DialogPreset preset, Citizen saysTo, SideJob jobRef)
        {
            // Check if this citizen has sold items to us before, and if one of them was sold more or equal than 5 times
            if (saysTo == null || !saysTo.isAtWork) return false;
            // TODO: Check also if the player has a free inventory slot!
            if (!RelationManager.Instance.PlayerInterests.PurchasedItemsFrom.TryGetValue(saysTo.humanID, out var items))
                return false;
            return items.Any(a => a.Value >= 5);
        }

        public void OnDialogExecute(DialogController instance, Citizen saysTo, Interactable saysToInteractable, NewNode where, Actor saidBy, bool success, NewRoom roomRef, SideJob jobRef)
        {
            // Collect most purchased item
            if (success && _item != null)
            {
                // TODO: Give item to the player

                // Pay for the item if its not free, and provide a custom response
                if (_item.Price > 0)
                {
                    GameplayController.Instance.AddMoney(-_item.Price, false, "Purchase");
                    saysTo.speechController.Speak(_item.Discount ? _positiveDiscountResponse.ToString() : _positiveResponse.ToString());
                }
                else if (_item.Price == 0)
                {
                    saysTo.speechController.Speak(_positiveFreeResponse.ToString());
                }

                Plugin.Log.LogInfo("Received item: " + _item.Name);

                // Reset to null for next dialogue
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

            // Check for discounts
            var rand = Toolbox.Instance.Rand(0, 100, true);
            if (rand < FreeChance)
                _item.Price = 0;
            else if (rand < DiscountChance)
            {
                _item.Price -= (int)Math.Round(_item.Price / 100 * (float)DiscountPercentage);
                _item.Discount = true;
            }

            // Make sure we don't go below 0
            if (_item.Price < 0)
                _item.Price = 0;

            // If we don't have enough money its a fail!
            if (GameplayController.Instance.money < _item.Price)
                return DialogController.ForceSuccess.fail;

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
            public int Price { get; set; }
            public bool Discount { get; set; } = false;

            public Item(string itemName, Interactable interactable, int price)
            {
                Name = itemName;
                Interactable = interactable;
                Price = price;
            }
        }
    }
}
