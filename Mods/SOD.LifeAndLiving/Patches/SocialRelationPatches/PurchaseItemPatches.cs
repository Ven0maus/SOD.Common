using HarmonyLib;
using SOD.Common.Extensions;
using SOD.LifeAndLiving.Relations;
using System;
using System.Linq;

namespace SOD.LifeAndLiving.Patches.SocialRelationPatches
{
    internal class PurchaseItemPatches
    {
        private static Actor _purchasedFrom = null;
        private static Action _resetFieldAction = null;

        [HarmonyPatch(typeof(ShopSelectButtonController), nameof(ShopSelectButtonController.PurchaseExecute))]
        internal static class ShopSelectButtonController_PurchaseExecute
        {
            [HarmonyPrefix]
            internal static void Prefix(out int __state)
            {
                // Apparently there is a check to see if it bought or not, so we try to reproduce it here to get accurate information
                __state = FirstPersonItemController.Instance.slots
                    .Select(a => a)
                    .Count(a => a.isStatic == FirstPersonItemController.InventorySlot.StaticSlot.nonStatic && a.GetInteractable() == null);
            }

            [HarmonyPostfix]
            internal static void Postfix(ShopSelectButtonController __instance, int __state)
            {
                var slots = FirstPersonItemController.Instance.slots
                    .Select(a => a)
                    .Count(a => a.isStatic == FirstPersonItemController.InventorySlot.StaticSlot.nonStatic && a.GetInteractable() == null);
                if (slots == __state)
                {
                    _purchasedFrom = null;
                    return; // We didn't buy anything
                }

                var company = Player.Instance.currentGameLocation?.thisAsAddress?.company;
                if (company != null)
                {
                    // Record where we bought and what we bought.
                    RelationManager.Instance.PlayerInterests.RecordPurchasedItem(company.companyID, __instance.preset.presetName, _purchasedFrom);
                }

                if (_purchasedFrom != null)
                {
                    if (_resetFieldAction == null)
                    {
                        _resetFieldAction = () =>
                        {
                            _purchasedFrom = null;
                            __instance.thisWindow.remove_OnWindowClosed(_resetFieldAction);
                            _resetFieldAction = null;
                        };

                        // Reset _purchasedFrom to null when the window is closed
                        __instance.thisWindow.add_OnWindowClosed(_resetFieldAction);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(DialogController), nameof(DialogController.BuySomething))]
        internal static class DialogController_BuySomething
        {
            [HarmonyPrefix]
            internal static void Prefix(Citizen saysTo, bool success)
            {
                if (!success)
                {
                    _purchasedFrom = null;
                    return;
                }
                _purchasedFrom = saysTo;
            }
        }
    }
}
