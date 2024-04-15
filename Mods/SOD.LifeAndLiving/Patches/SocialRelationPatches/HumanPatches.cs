using HarmonyLib;
using SOD.LifeAndLiving.Relations;
using System;
using System.Collections.Generic;
using UnityEngine;

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
                if (citizen == null || __instance.isMachine || !citizen.isPlayer) return;

                float distance = Vector3.Distance(__instance.lookAtThisTransform.position, citizen.lookAtThisTransform.position);
                if (distance < GameplayControls.Instance.minimumStealthDetectionRange ||
                    __instance.ActorRaycastCheck(citizen, distance + 3f, out _, false, Color.green, Color.red, Color.white, 1f))
                {
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
}
