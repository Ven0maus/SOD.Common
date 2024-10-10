using HarmonyLib;
using System;
using System.Collections;

namespace SOD.Narcotics.Patches
{
    internal class TakeOnePatches
    {
        [HarmonyPatch(typeof(FirstPersonItemController), "TakeOne")]
        internal static class FirstPersonItemController_TakeOne
        {
            /// <summary>
            /// Raised when a consumable is consumed.
            /// </summary>
            internal static event EventHandler<Interactable> OnItemConsumed;

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

                if (Plugin.Instance.Config.DebugMode)
                    Plugin.Log.LogInfo($"Finished consuming consumable \"{consumable.name}\".");
            }
        }
    }
}
