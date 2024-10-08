﻿using HarmonyLib;

namespace SOD.Common.Patches
{
    internal class GameplayPatches
    {
        [HarmonyPatch(typeof(Human.Death), nameof(Human.Death.SetReported))]
        internal class Death_SetReported
        {
            private static bool _notReported = false;

            [HarmonyPrefix]
            internal static void Prefix(Human.Death __instance)
            {
                if (!__instance.reported)
                {
                    _notReported = true;
                }
            }

            [HarmonyPostfix]
            internal static void Postfix(Human.Death __instance, Human newFoundBy, Human.Death.ReportType newReportType)
            {
                if (_notReported)
                {
                    _notReported = false;

                    // Raise method for victim discovery
                    var victim = __instance.GetVictim();
                    if (victim != null)
                        Lib.Gameplay.VictimReported(victim, newFoundBy, newReportType);
                }
            }
        }

        [HarmonyPatch(typeof(MurderController), nameof(MurderController.OnVictimKilled))]
        internal class MurderController_OnVictimKilled
        {
            [HarmonyPrefix]
            internal static void Prefix(MurderController __instance)
            {
                var murder = GetMurder(__instance);
                if (murder == null) return;
                Lib.Gameplay.VictimKilled(murder);
            }

            private static MurderController.Murder GetMurder(MurderController murderController)
            {
                foreach (var murder in murderController.activeMurders)
                {
                    if (murder.state == MurderController.MurderState.executing && murder.victim.isDead)
                        return murder;
                }
                return null;
            }
        }

        [HarmonyPatch(typeof(FirstPersonItemController), nameof(FirstPersonItemController.PickUpItem))]
        internal class FirstPersonItemController_PickUpItem
        {
            [HarmonyPostfix]
            internal static void Postfix(Interactable pickUpThis, ref bool __result)
            {
                if (__result)
                    Lib.Gameplay.InteractablePickedUp(pickUpThis);
            }
        }

        [HarmonyPatch(typeof(InteractableController), nameof(InteractableController.DropThis))]
        internal class InteractableController_DropThis
        {
            [HarmonyPostfix]
            internal static void Postfix(InteractableController __instance, bool throwThis)
            {
                Lib.Gameplay.InteractableDropped(__instance.interactable, throwThis && __instance.rb != null);
            }
        }
    }
}
