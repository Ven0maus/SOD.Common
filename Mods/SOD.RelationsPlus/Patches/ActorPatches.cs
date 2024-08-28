using HarmonyLib;
using System;

namespace SOD.RelationsPlus.Patches
{
    internal class ActorPatches
    {
        [HarmonyPatch(typeof(Actor), nameof(Actor.RecieveDamage))]
        internal static class Actor_RecieveDamage
        {
            [HarmonyPostfix]
            internal static void Prefix(Actor __instance, ref float __state)
            {
                __state = __instance.currentHealth;
            }

            internal static void Postfix(Actor __instance, float amount, Actor fromWho, ref float __state)
            {
                if (amount == 0f || __instance.currentHealth <= 0f || __instance.isStunned || __instance.currentHealth == __state) return;

                if (__instance.isPlayer) return;
                if (fromWho == null || !fromWho.isPlayer) return;

                Human human = null;
                try
                {
                    human = __instance.TryCast<Human>();
                }
                catch (InvalidCastException)
                { }

                if (human == null) return;

                if (Plugin.Instance.Config.DebugMode)
                    Plugin.Log.LogInfo($"[Debug]: {human.GetCitizenName()} was attacked by player!");

                RelationManager.Instance[human.humanID].Like += Plugin.Instance.Config.OnAttackCitizenModifier;
            }
        }
    }
}
