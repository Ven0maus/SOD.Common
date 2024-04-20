using SOD.Common;
using SOD.Common.Helpers.DialogObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SOD.LifeAndLiving.Relations.Dialogs
{
    internal class TheUsualDialog : IDialogLogic
    {
        private static Guid _positiveResponse;
        private static Guid _positiveDiscountResponse;
        private static Guid _positiveFreeResponse;

        private static Item _item = null;
        private readonly Dictionary<int, (string Hash, bool Received)> _discountCache = new();

        /// <summary>
        /// Add's several dialog options regarding purchasing items often at places.
        /// </summary>
        internal static void Register()
        {
            // For giggles?
            var customPositiveText = () => Player.Instance.gender == Human.Gender.male ?
                $"Good choice sir, one {_item?.Name ?? ""} coming right up!" : Player.Instance.gender == Human.Gender.female ?
                $"Good choice ma'am, one {_item?.Name ?? ""} coming right up!" :
                $"Good choice, one {_item?.Name ?? ""} coming right up!";

            _ = Lib.Dialog.Builder()
                .SetText("For me, the usual as always.")
                .AddCustomResponse(customPositiveText, out _positiveResponse)
                .AddCustomResponse(() => $"You're lucky, this {_item?.Name ?? "item"} has a {Plugin.Instance.Config.TheUsualDiscountValue}% discount!", out _positiveDiscountResponse)
                .AddCustomResponse(() => $"Coming right up, this {_item?.Name ?? "item"} is on the house!", out _positiveFreeResponse)
                .AddResponse(() => $"Looks like you can't afford {_item?.Name ?? "it"} today.", isSuccesful: false)
                .ModifyDialogOptions((a) =>
                {
                    a.useSuccessTest = true;
                    a.ranking = int.MaxValue;
                })
                .SetDialogLogic(new TheUsualDialog())
                .CreateAndRegister();
        }

        public bool IsDialogShown(DialogPreset preset, Citizen saysTo, SideJob jobRef)
        {
            // Check if they have seen the player at work 4 or more times
            if (!RelationManager.Instance.Exists(saysTo.humanID) || RelationManager.Instance[saysTo.humanID].SeenAtWork < 4) 
                return false;

            // Check if the citizen is at work and if the player has a free slot available
            if (saysTo == null || !saysTo.isAtWork || saysTo.job?.employer == null || !FirstPersonItemController.Instance.IsSlotAvailable())
                return false;

            // Check if we have purchased items from this citizen
            if (!RelationManager.Instance.PlayerInterests.PurchasedItemsFrom.TryGetValue(saysTo.humanID, out var items))
                return false;

            // Check if any item was purchased 4 or more times by the player
            return items.Any(a => a.Value >= 4);
        }

        public void OnDialogExecute(DialogController instance, Citizen saysTo, Interactable saysToInteractable, NewNode where, Actor saidBy, bool success, NewRoom roomRef, SideJob jobRef)
        {
            // Collect most purchased item
            if (success && _item != null)
            {
                // Spawn interactable to give to the player
                Interactable interactable = InteractableCreator.Instance.CreateWorldInteractable(
                    _item.Preset, 
                    Player.Instance, 
                    Player.Instance, 
                    null, 
                    Player.Instance.transform.position + new UnityEngine.Vector3(0f, 3.5f, 0f),
                    Player.Instance.transform.eulerAngles, null, null, "");
                if (interactable != null)
                {
                    HandleInteractableLogic(interactable);

                    // Pay for the item if its not free, and provide a custom response
                    if (_item.Price > 0)
                    {
                        GameplayController.Instance.AddMoney(-_item.Price, false, "Purchase");
                        saysTo.speechController.Speak("dds.blocks", _item.Discount ? 
                            _positiveDiscountResponse.ToString() : _positiveResponse.ToString(), endsDialog: true);
                    }
                    else if (_item.Price == 0)
                    {
                        saysTo.speechController.Speak("dds.blocks", _positiveFreeResponse.ToString(), endsDialog: true);
                    }
                }
                else
                {
                    Plugin.Log.LogInfo("Could not create interactable.");
                }
            }

            // Reset to null for next dialogue
            _item = null;
        }

        private static void HandleInteractableLogic(Interactable interactable)
        {
            interactable.SetSpawnPositionRelevent(false);
            if (Player.Instance.currentGameLocation != null &&
                Player.Instance.currentGameLocation.thisAsAddress != null &&
                Player.Instance.currentGameLocation.thisAsAddress.company != null &&
                Player.Instance.currentGameLocation.thisAsAddress.company.preset.enableLoiteringBehaviour)
            {
                Player.Instance.currentGameLocation.LoiteringPurchase();
            }

            if (!FirstPersonItemController.Instance.PickUpItem(interactable, true, false, true, true, false))
            {
                Plugin.Log.LogInfo("Unable to pickup item: " + interactable.name);
                interactable.Delete();
                return;
            }
            AudioController.Instance.Play2DSound(AudioControls.Instance.purchaseItem, null, 1f);
            interactable.MarkAsTrash(true, false, 0f);
        }

        public DialogController.ForceSuccess ShouldDialogSucceedOverride(DialogController instance, EvidenceWitness.DialogOption dialog, Citizen saysTo, NewNode where, Actor saidBy)
        {
            if (!saysTo)
                return DialogController.ForceSuccess.none;

            _item = CollectMostPurchasedItem(RelationManager.Instance.PlayerInterests.PurchasedItemsFrom[saysTo.humanID], saysTo.job.employer);
            if (_item == null)
            {
                Plugin.Log.LogInfo("Could not find item, this should not have happened.");
                return DialogController.ForceSuccess.fail;
            }

            var currentTime = Lib.Time.CurrentDateTime;
            var hash = Lib.SaveGame.GetUniqueString($"{currentTime.Year}_{currentTime.Month}_{currentTime.Day}_{currentTime.Hour}_{saysTo.humanID}");

            // Check if this npc gave a discount before for this hash
            if (!_discountCache.TryGetValue(saysTo.humanID, out var discount))
                _discountCache[saysTo.humanID] = discount = (hash, false);

            if (discount.Hash != hash)
            {
                // Have another chance at a discount or free
                _discountCache[saysTo.humanID] = discount = (hash, false);
            }

            // Check for discounts
            if (!discount.Received)
            {
                // Use a new random with a custom hash seed based on the in-game time and citizen
                // So u cannot just spam the dialog until you get lucky, but that its predetermined what you get per in game hour.
                var rand = new Random(hash.GetHashCode()).Next(0, 100);
                if (rand < Plugin.Instance.Config.TheUsualFreeChance)
                {
                    _item.Price = 0;
                    _discountCache[saysTo.humanID] = (hash, true);
                }
                else if (rand < Plugin.Instance.Config.TheUsualDiscountChance)
                {
                    _item.Price -= (int)Math.Round(_item.Price / 100 * (float)Plugin.Instance.Config.TheUsualDiscountValue);
                    _item.Discount = true;
                    _discountCache[saysTo.humanID] = (hash, true);
                }
            }

            // Make sure we don't go below 0
            if (_item.Price < 0)
                _item.Price = 0;

            // If we don't have enough money its a fail!
            if (GameplayController.Instance.money < _item.Price)
                return DialogController.ForceSuccess.fail;

            return DialogController.ForceSuccess.none;
        }

        private static Item CollectMostPurchasedItem(Dictionary<string, int> items, Company company)
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
                    var itemCode = Lib.SaveGame.GetUniqueString(companyId + "_" + item.Key);
                    if (mostPurchasedItemCode == itemCode)
                    {
                        itemName = item.Key;
                        break;
                    }
                }
                if (itemName != null)
                    break;
            }

            return GetItem(itemName, company);
        }

        private static Item GetItem(string itemName, Company company)
        {
            if (itemName == null) return null;

            foreach (var item in company.prices)
            {
                if (item.key.presetName == itemName)
                {
                    return new Item(itemName, item.Key, item.Value);
                }
            }

            return null;
        }

        class Item
        {
            public string Name { get; }
            public InteractablePreset Preset { get; }
            public int Price { get; set; }
            public bool Discount { get; set; } = false;

            public Item(string itemName, InteractablePreset preset, int price)
            {
                Name = itemName;
                Preset = preset;
                Price = price;
            }
        }
    }
}
