using HarmonyLib;

namespace SOD.LethalActionReborn.Patches
{
    internal class CitizenPatches
    {
        [HarmonyPatch(typeof(Citizen), nameof(Citizen.RecieveDamage))]
        internal static class Citizen_RecieveDamage
        {
            private static bool _hasKilled = false;

            [HarmonyPrefix]
            internal static void Prefix(Citizen __instance, float amount, Actor fromWho)
            {
                if (fromWho == Player.Instance)
                    _hasKilled = __instance.currentHealth - amount <= 0;
            }

            [HarmonyPostfix]
            internal static void Postfix(Citizen __instance, Actor fromWho)
            {
                if (!__instance.isDead && _hasKilled && fromWho == Player.Instance)
                {
                    _hasKilled = false;
                    __instance.isStunned = false;
                    __instance.SetHealth(0f);
                    //__instance.ai.SetKO(true, default, default, false, 0f, true, 1f);
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
