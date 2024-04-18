using HarmonyLib;
using SOD.LifeAndLiving.Relations;
using System;
using UnityEngine;

namespace SOD.LifeAndLiving.Patches.SocialRelationPatches
{
    internal class HumanPatches
    {
        [HarmonyPatch(typeof(Human), nameof(Human.UpdateLastSighting))]
        internal static class Human_UpdateLastSighting
        {
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
                    var isInTheSameRoom = __instance.currentRoom != null && citizen.currentRoom != null && __instance.currentRoom.roomID == citizen.currentRoom.roomID;
                    var isInTheSameBuilding = __instance.currentBuilding != null && citizen.currentBuilding != null && __instance.currentBuilding.buildingID == citizen.currentBuilding.buildingID;

                    var relation = RelationManager.Instance[__instance.humanID];
                    if (relation.LastSeen == null || relation.LastSeen.Value.AddMinutes(1) < DateTime.Now)
                    {
                        if (__instance.isAtWork && isInTheSameRoom)
                        {
                            relation.LastSeen = DateTime.Now;
                            relation.SeenAtWork++;
                        }
                        else if (__instance.isHome && isInTheSameBuilding)
                        {
                            relation.LastSeen = DateTime.Now;
                            relation.SeenAtHome++;
                        }
                        else if ((__instance.isOnStreet && citizen.isOnStreet) || isInTheSameRoom)
                        {
                            relation.LastSeen = DateTime.Now;
                            relation.SeenOutsideOfWork++;
                        }
                    }
                }
            }
        }
    }
}
