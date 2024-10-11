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
                public bool State { get; set; }
            }

            private static Consumable _consumable;

            [HarmonyPrefix]
            private static void Prefix(FirstPersonItemController __instance, ref Consumable __state)
            {
                __state = _consumable ??= new Consumable();
                __state.Interactable = BioScreenController.Instance.selectedSlot != null && BioScreenController.Instance.selectedSlot.interactableID > -1 ?
                        BioScreenController.Instance.selectedSlot.GetInteractable() : null;
                __state.State = __instance.isConsuming && !__instance.takeOneActive;
            }

            [HarmonyPostfix]
            private static void Postfix(FirstPersonItemController __instance, ref Consumable __state)
            {
                if (SessionData.Instance.play)
                {
                    if (__instance.isConsuming && !__instance.takeOneActive && __state.Interactable != null)
                    {
                        Interactable interactable = __state.Interactable;
                        if (interactable.cs > 0f)
                        {
                            // Keep track of how much we consumed per type and potency
                            // if we've consumed enough reset the consumation and apply addiction consumption
                            var addictionInfo = AddictionManager.GetAddictionTypeAndPotency(interactable);
                            if (addictionInfo != null)
                            {
                                // The full potency for consuming the entire item is basically == to the full consumable amount
                                // So to calculate how much 1 cs would equal we divide the potency by the full consumable amount
                                var potency = addictionInfo.Value.potency ?? 1f;
                                potency /= interactable.preset.consumableAmount;
                                AddictionManager.AddConsumptionRate(addictionInfo.Value.addictionType, UnityEngine.Time.deltaTime, potency);
                            }
                        }
                    }
                    else if (__state.State && __state.Interactable != null)
                    {
                        // We stopped consuming$
                        if (Plugin.Instance.Config.DebugMode)
                            Plugin.Log.LogInfo("Stopped consuming: " + __state.Interactable.name);
                    }
                }
            }
        }
    }
}
