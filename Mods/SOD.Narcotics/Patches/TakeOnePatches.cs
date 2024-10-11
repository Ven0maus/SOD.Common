using HarmonyLib;
using SOD.Narcotics.AddictionCore;
using System;
using System.Collections;

namespace SOD.Narcotics.Patches
{
    internal class TakeOnePatches
    {
        /// <summary>
        /// Raised when a consumable is consumed.
        /// </summary>
        internal static event EventHandler<Interactable> OnItemConsumed;

        [HarmonyPatch(typeof(FirstPersonItemController), "TakeOne")]
        internal static class FirstPersonItemController_TakeOne
        {
            [HarmonyPrefix]
            private static void Prefix(FirstPersonItemController __instance, ref bool __state)
            {
                __state = !__instance.takeOneActive;
            }

            [HarmonyPostfix]
            private static void Postfix(FirstPersonItemController __instance, ref bool __state)
            {
                if (__state && __instance.takeOneActive)
                {
                    if (BioScreenController.Instance.selectedSlot == null || BioScreenController.Instance.selectedSlot.interactableID <= -1)
                        return;

                    UniverseLib.RuntimeHelper.StartCoroutine(CheckForCompletion(__instance, BioScreenController.Instance.selectedSlot.GetInteractable()));
                }
            }

            private static IEnumerator CheckForCompletion(FirstPersonItemController instance, Interactable consumable)
            {
                while (instance.takeOneActive)
                    yield return null;

                OnItemConsumed?.Invoke(instance, consumable);
            }
        }

        [HarmonyPatch(typeof(FirstPersonItemController), "Update")]
        internal static class FirstPersonItemController_Update
        {
            class Consumable
            {
                public Interactable Interactable { get; set; }
                public bool IsConsuming { get; set; }
                public float RemainingAmount { get; set; }
            }

            private static Consumable _consumable = new Consumable();

            [HarmonyPrefix]
            private static void Prefix(FirstPersonItemController __instance, ref Consumable __state)
            {
                __state ??= _consumable;
                if (!__state.IsConsuming && __instance.isConsuming && !__instance.takeOneActive)
                {
                    if (BioScreenController.Instance.selectedSlot != null && BioScreenController.Instance.selectedSlot.interactableID > -1)
                    {
                        if (__state.Interactable == null || __state.Interactable.id != BioScreenController.Instance.selectedSlot.interactableID)
                            __state.Interactable = BioScreenController.Instance.selectedSlot.GetInteractable();
                    }
                    else
                    {
                        __state.Interactable = null;
                    }
                    __state.IsConsuming = true;
                    __state.RemainingAmount = __state.Interactable?.cs ?? 0f;
                }
            }

            [HarmonyPostfix]
            private static void Postfix(FirstPersonItemController __instance, ref Consumable __state)
            {
                if (SessionData.Instance.play)
                {
                    // We stopped consuming
                    if (__state.IsConsuming && (!__instance.isConsuming || __instance.takeOneActive)) 
                    {
                        __state.IsConsuming = false;

                        var fullAmount = __state.Interactable.preset.consumableAmount;
                        var leftOverPercentage = __state.RemainingAmount / fullAmount;
                        var consumedPercentage = leftOverPercentage - (__state.Interactable.cs / __state.RemainingAmount);
                        __state.RemainingAmount = __state.Interactable.cs;

                        var addictionInfo = AddictionManager.GetAddictionTypeAndPotency(__state.Interactable);
                        if (addictionInfo != null)
                            AddictionManager.OnItemConsumed(addictionInfo.Value.addictionType, consumedPercentage, addictionInfo.Value.potency);
                    }
                }
            }
        }
    }
}
