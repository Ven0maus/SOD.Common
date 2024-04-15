using HarmonyLib;
using SOD.LifeAndLiving.Relations;
using System;
using System.Collections.Generic;

namespace SOD.LifeAndLiving.Patches.SocialRelationPatches
{
    internal class HumanPatches
    {
        [HarmonyPatch(typeof(Human), nameof(Human.UpdateLastSighting))]
        internal static class Human_UpdateLastSighting
        {
            private static readonly Dictionary<int, DateTime> _lastSeenTimings = new();

            [HarmonyPrefix]
            internal static void Prefix(Human __instance, Human citizen)
            {
                // This method is called on a citizen when he sees another citizen, in this case
                // We are only interested when they see the player
                if (!citizen.isPlayer) return;

                if (!_lastSeenTimings.TryGetValue(__instance.humanID, out var time))
                {
                    _lastSeenTimings.Add(__instance.humanID, time = DateTime.Now.AddMinutes(-2));
                }
                
                if (time.AddMinutes(1) < DateTime.Now)
                {
                    _lastSeenTimings[__instance.humanID] = DateTime.Now;
                    var relation = RelationManager.Instance[__instance.humanID];
                    relation.LastSeen = time;
                    relation.Seen++;
                }
            }
        }
    }
}
