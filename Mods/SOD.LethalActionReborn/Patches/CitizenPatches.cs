using HarmonyLib;
using System.Collections.Generic;
using System.Linq;

namespace SOD.LethalActionReborn.Patches
{
    internal class CitizenPatches
    {
        [HarmonyPatch(typeof(Citizen), nameof(Citizen.RecieveDamage))]
        internal static class Citizen_RecieveDamage
        {
            internal static readonly HashSet<int> KilledCitizens = new();
            private static bool _hasKilled = false;

            internal static HashSet<MurderWeaponPreset.WeaponType> ExcludedWeaponTypes = null;
            internal static Dictionary<int, int> CitizenHitsTakenOnKo = new();

            [HarmonyPrefix]
            internal static void Prefix(Citizen __instance, Actor fromWho)
            {
                if (fromWho == Player.Instance && __instance.ai.ko)
                {
                    if (ExcludedWeaponTypes != null && ExcludedWeaponTypes.Any() && BioScreenController.Instance.selectedSlot != null)
                    {
                        var weapon = BioScreenController.Instance.selectedSlot.GetInteractable();
                        if (weapon != null && weapon.preset.weapon != null)
                        {
                            var wPreset = weapon.preset.weapon;
                            if (ExcludedWeaponTypes.Contains(wPreset.type))
                            {
                                _hasKilled = false;
                                return;
                            }
                        }
                    }

                    CitizenHitsTakenOnKo.TryGetValue(__instance.humanID, out int hitsTaken);
                    hitsTaken += 1;
                    CitizenHitsTakenOnKo[__instance.humanID] = hitsTaken;

                    if (hitsTaken >= Plugin.Instance.Config.HitsRequiredForKillAfterKo)
                        _hasKilled = true;
                }
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

        [HarmonyPatch(typeof(NewAIController), nameof(NewAIController.SetKO))]
        internal static class NewAIController_SetKO
        {
            [HarmonyPostfix]
            internal static void Postfix(NewAIController __instance, bool val)
            {
                if (!val)
                    Citizen_RecieveDamage.CitizenHitsTakenOnKo.Remove(__instance.human.humanID);
            }
        }
    }
}
