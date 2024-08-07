﻿using HarmonyLib;
using System;

namespace SOD.LifeAndLiving.Patches.EconomyRebalancePatches
{
    internal class BuyInterfacePatches
    {
        [HarmonyPatch(typeof(BuyInterfaceController), nameof(BuyInterfaceController.UpdateElements))]
        internal class BuyInterfaceController_UpdateElements
        {
            // Limit item sale prices

            [HarmonyPostfix]
            internal static void Postfix(BuyInterfaceController __instance)
            {
                var value = __instance.spawned;

                var isIllegal = __instance.company != null && __instance.company.preset.enableSellingOfIllegalItems;
                var maxSellPriceGeneral = Plugin.Instance.Config.MaxSellPriceAllItemsGeneral;
                var maxSellPriceBlackMarket = Plugin.Instance.Config.MaxSellPriceAllItemsBlackMarket;

                bool updates = false;
                int count = 0;
                foreach (var component in value)
                {
                    // Only for sale prices
                    if (component.sellInteractable != null && component.sellMode && !component.sellInteractable.preset.presetName.Equals("Diamond"))
                    {
                        var prev = component.price;

                        component.price = Math.Min(component.price, isIllegal ? maxSellPriceBlackMarket : maxSellPriceGeneral);
                        if (prev != component.price)
                        {
                            component.UpdateButtonText();
                            updates = true;
                            count++;
                        }
                    }
                }

                if (updates)
                {
                    __instance.UpdatePurchaseAbility();
                    Plugin.Log.LogInfo($"Capped sale price of \"{count}\" items at store interface.");
                }
            }
        }
    }
}
