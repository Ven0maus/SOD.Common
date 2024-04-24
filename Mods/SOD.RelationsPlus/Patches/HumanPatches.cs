using HarmonyLib;
using System;
using UnityEngine;

namespace SOD.RelationsPlus.Patches
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
                var player = citizen;
                if (player == null || __instance.isMachine || !player.isPlayer) return;

                var relation = RelationManager.Instance[__instance.humanID];
                if (relation.Visibility.LastSeen == null || relation.Visibility.LastSeen.Value.AddSeconds(45) < DateTime.Now)
                {
                    float distance = Vector3.Distance(__instance.lookAtThisTransform.position, player.lookAtThisTransform.position);
                    if (distance < GameplayControls.Instance.minimumStealthDetectionRange ||
                        __instance.ActorRaycastCheck(player, distance + 3f, out _, false, Color.green, Color.red, Color.white, 1f))
                    {
                        var isInTheSameRoom = __instance.currentRoom != null &&
                            player.currentRoom != null && 
                            __instance.currentRoom.roomID == player.currentRoom.roomID;
                        
                        var isInTheSameBuilding = __instance.currentBuilding != null &&
                            player.currentBuilding != null && 
                            __instance.currentBuilding.buildingID == player.currentBuilding.buildingID;

                        var isInTheSameHomeBuilding = __instance.home != null &&
                            player.currentGameLocation != null &&
                            player.currentGameLocation.thisAsAddress != null &&
                            player.currentGameLocation.thisAsAddress == __instance.home;

                        bool seen = false;
                        if (__instance.isAtWork && isInTheSameRoom)
                        {
                            relation.Visibility.SeenAtWork++;
                            seen = true;
                        }
                        else if (__instance.isHome && isInTheSameRoom)
                        {
                            relation.Visibility.SeenAtHome++;
                            seen = true;
                        }
                        else if (isInTheSameHomeBuilding)
                        {
                            relation.Visibility.SeenAtHomeBuilding++;
                            seen = true;
                        }
                        else if ((__instance.isOnStreet && player.isOnStreet) || isInTheSameRoom || isInTheSameBuilding)
                        {
                            relation.Visibility.SeenOutsideOfWork++;
                            seen = true;
                        }

                        if (seen)
                        {
                            if (player.isTrespassing)
                                relation.Like -= 0.05f;
                            relation.Know += 0.035f;
                            relation.Visibility.LastSeen = DateTime.Now;
                        }
                    }
                }
            }
        }
    }
}