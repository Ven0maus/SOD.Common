using HarmonyLib;
using SOD.RelationsPlus.Objects;
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

                var relationExists = RelationManager.Instance.TryGetValue(__instance.humanID, out var relation);
                if (!relationExists || 
                    relation.LastSeenRealTime == null || 
                    relation.LastSeenRealTime.Value.AddSeconds(45) < DateTime.Now)
                {
                    float distance = Vector3.Distance(__instance.lookAtThisTransform.position, player.lookAtThisTransform.position);
                    if (distance < GameplayControls.Instance.minimumStealthDetectionRange ||
                        __instance.ActorRaycastCheck(player, distance + 3f, out _, false, Color.green, Color.red, Color.white, 1f))
                    {
                        // If relation doesn't exist yet at this point, create a new one
                        relation ??= new CitizenRelation(__instance.humanID);

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

                        float previousKnow = relation.Know;
                        float previousLike = relation.Like;

                        SeenPlayerArgs.SeenLocation location = (SeenPlayerArgs.SeenLocation)(-1);
                        if (__instance.isAtWork && isInTheSameRoom)
                        {
                            relation.Know += 0.025f;
                            location = SeenPlayerArgs.SeenLocation.Workplace;
                        }
                        else if (__instance.isHome && isInTheSameRoom)
                        {
                            relation.Know += 0.045f;
                            location = SeenPlayerArgs.SeenLocation.Home;
                        }
                        else if (isInTheSameHomeBuilding)
                        {
                            relation.Know += 0.035f;
                            location = SeenPlayerArgs.SeenLocation.HomeBuilding;
                        }
                        else if ((__instance.isOnStreet && player.isOnStreet) || isInTheSameRoom || isInTheSameBuilding)
                        {
                            relation.Know += 0.015f;
                            location = SeenPlayerArgs.SeenLocation.Street;
                        }

                        // Player was seen by citizen
                        if ((int)location != -1)
                        {
                            if (player.isTrespassing)
                                relation.Like -= 0.05f;

                            // Add it to the matrix if it didn't exist yet
                            if (!relationExists)
                                RelationManager.Instance.AddOrReplace(relation);

                            relation.Seen(location, previousKnow - relation.Know, previousLike - relation.Like);
                        }
                    }
                }
            }
        }
    }
}