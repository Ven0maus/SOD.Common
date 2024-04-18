using SOD.Common.Helpers.DialogObjects;
using SOD.LifeAndLiving.Relations;
using System;
using System.Linq;

namespace SOD.LifeAndLiving.Patches.SocialRelationPatches.DialogLogic
{
    internal class TheUsualDialogLogic : IDialogLogic
    {
        public bool IsDialogShown(DialogPreset preset, Citizen saysTo, SideJob jobRef)
        {
            // Check if this citizen has sold items to us before, and if one of them was sold more or equal than 5 times
            if (!RelationManager.Instance.PlayerInterests.PurchasedItemsFrom.TryGetValue(saysTo.humanID, out var items))
                return false;
            return items.Any(a => a.Value >= 5);
        }

        public void OnDialogExecute(DialogController instance, Citizen saysTo, Interactable saysToInteractable, NewNode where, Actor saidBy, bool success, NewRoom roomRef, SideJob jobRef)
        {
            // Collect most purchased item
            var items = RelationManager.Instance.PlayerInterests.PurchasedItemsFrom[saysTo.humanID];
            var mostPurchasedItemCode = items.Where(a => a.Value >= 5).OrderByDescending(a => a.Value).First().Key;

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

            if (itemName != null)
            {
                // TODO: Buy this item for the player
                // Have a small chance to receive a discounted price, or a free one.
            }
        }

        public DialogController.ForceSuccess ShouldDialogSucceedOverride(DialogController instance, EvidenceWitness.DialogOption dialog, Citizen saysTo, NewNode where, Actor saidBy)
        {
            return DialogController.ForceSuccess.success;
        }
    }
}
