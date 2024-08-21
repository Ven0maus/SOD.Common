using HarmonyLib;
using System.Collections.Generic;

namespace SOD.LethalActionReborn.Patches
{
    internal class CitizenPatches
    {
        [HarmonyPatch(typeof(Citizen), nameof(Citizen.RecieveDamage))]
        internal static class Citizen_RecieveDamage
        {
            internal static readonly HashSet<int> KilledCitizens = new();
            private static bool _hasKilled = false;

            [HarmonyPrefix]
            internal static void Prefix(Citizen __instance, float amount, Actor fromWho)
            {
                if (fromWho == Player.Instance && __instance.ai.ko)
                    _hasKilled = true;
            }

            [HarmonyPostfix]
            internal static void Postfix(Citizen __instance, Actor fromWho)
            {
                if (!__instance.isDead && _hasKilled && fromWho == Player.Instance)
                {
                    _hasKilled = false;
                    __instance.SetHealth(0f);
                    __instance.animationController.cit.Murder(Player.Instance, false, null, null);
                    CityData.Instance.deadCitizensDirectory.Add(__instance);
                    KilledCitizens.Add(__instance.humanID);

                    int check = 0;
                    while (MurderController.Instance.currentMurderer != null && MurderController.Instance.currentMurderer.humanID == __instance.humanID)
                    {
                        if (check >= 500) break;
                        check++;
                        MurderController.Instance.currentMurderer = null;
                        MurderController.Instance.PickNewMurderer();
                    }

                    check = 0;
                    while (MurderController.Instance.currentVictim != null && MurderController.Instance.currentVictim.humanID == __instance.humanID)
                    {
                        if (check >= 500) break;
                        check++;
                        MurderController.Instance.currentVictim = null;
                        MurderController.Instance.PickNewVictim();
                    }
                }
            }
        }
    }
}
