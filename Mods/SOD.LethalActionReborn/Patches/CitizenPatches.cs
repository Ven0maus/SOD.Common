using HarmonyLib;

namespace SOD.LethalActionReborn.Patches
{
    internal class CitizenPatches
    {
        [HarmonyPatch(typeof(Citizen), nameof(Citizen.RecieveDamage))]
        internal static class Citizen_RecieveDamage
        {
            [HarmonyPostfix]
            internal static void Postfix(Citizen __instance, float amount, Actor fromWho)
            {
                if (!__instance.isDead && __instance.currentHealth - amount <= 0 && fromWho == Player.Instance)
                {
                    __instance.isStunned = false;
                    __instance.SetHealth(0f);
                    __instance.ai.SetKO(true, default, default, false, 0f, true, 1f);
                    __instance.animationController.cit.Murder(Player.Instance, false, null, null);
                }

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
