using HarmonyLib;

namespace SOD.LethalActionReborn.Patches
{
    internal class CitizenPatches
    {
        [HarmonyPatch(typeof(Citizen), nameof(Citizen.RecieveDamage))]
        internal static class Citizen_RecieveDamage
        {
            [HarmonyPrefix]
            internal static void Prefix(Citizen __instance, float amount, Actor fromWho, ref bool enableKill)
            {
                if (!__instance.isDead && __instance.currentHealth - amount <= 0 && fromWho == Player.Instance)
                {
                    __instance.isStunned = false;
                    if (!enableKill)
                        enableKill = true;
                }
            }

            [HarmonyPostfix]
            internal static void Postfix(Citizen __instance, Actor fromWho)
            {
                if (__instance.isDead && __instance.currentHealth <= 0 && fromWho == Player.Instance)
                {
                    if (MurderController.Instance.currentMurderer == __instance)
                    {
                        MurderController.Instance.PickNewMurderer();
                        MurderController.Instance.PickNewVictim();
                    }
                    else if (MurderController.Instance.currentVictim == __instance)
                    {
                        MurderController.Instance.PickNewVictim();
                    }
                }
            }
        }
    }
}
