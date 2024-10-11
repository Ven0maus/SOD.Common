using HarmonyLib;
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
                            
                        }
                    }
                    else if (__state.State && __state.Interactable != null)
                    {
                        // We stopped consuming
                        Plugin.Log.LogInfo("Stopped consuming: " + __state.Interactable.name);
                    }
                }
            }
        }
    }
}
