using HarmonyLib;
using SOD.Common;
using SOD.RelationsPlus.Objects;
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
                // This method is called on a citizen when he sees another citizen.
                // in this case we are only interested when they see the player
                var player = citizen;
                if (player == null || __instance.isMachine || !player.isPlayer) return;

                // Following scenarios are valid for the citizen to see the player:
                // - There is no relational information yet between citizen and the player
                // - The player was not trespassing last time when seen, but he is now
                // - The last time the player was seen by this citizen was more than "SeenTimeMinutesCheck" minutes ago.
                // - The distance between player / citizen is respected by the base game (stealth detection range, or raycast check)

                var relationExists = RelationManager.Instance.TryGetValue(__instance.humanID, out var relation);
                if (!relationExists || (!relation.WasTrespassingLastTimeSeen && player.isTrespassing) ||
                    relation.LastSeenGameTime == null || 
                    relation.LastSeenGameTime.Value.AddMinutes(Plugin.Instance.Config.SeenTimeMinutesCheck) <= Lib.Time.CurrentDateTime)
                {
                    float distance = Vector3.Distance(__instance.lookAtThisTransform.position, player.lookAtThisTransform.position);
                    if (distance < GameplayControls.Instance.minimumStealthDetectionRange ||
                        __instance.ActorRaycastCheck(player, distance + 3f, out _, false, Color.green, Color.red, Color.white, 1f))
                    {
                        // If relation doesn't exist yet at this point, create a new one
                        relation ??= new CitizenRelation(__instance.humanID);
                        relation.WasTrespassingLastTimeSeen = player.isTrespassing;

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
                            relation.Know += Plugin.Instance.Config.SeenAtWorkplaceModifier;
                            location = SeenPlayerArgs.SeenLocation.Workplace;
                        }
                        else if (__instance.isHome && isInTheSameRoom)
                        {
                            relation.Know += Plugin.Instance.Config.SeenInHomeModifier;
                            location = SeenPlayerArgs.SeenLocation.Home;
                        }
                        else if (isInTheSameHomeBuilding)
                        {
                            relation.Know += Plugin.Instance.Config.SeenInHomeBuildingModifier;
                            location = SeenPlayerArgs.SeenLocation.HomeBuilding;
                        }
                        else if ((__instance.isOnStreet && player.isOnStreet) || isInTheSameRoom || isInTheSameBuilding)
                        {
                            relation.Know += Plugin.Instance.Config.SeenOnStreetModifier;
                            location = SeenPlayerArgs.SeenLocation.Street;
                        }

                        // Player was seen by citizen
                        if ((int)location != -1)
                        {
                            if (player.isTrespassing)
                                relation.Like += Plugin.Instance.Config.SeenTrespassingModifier;

                            // Add it to the matrix if it didn't exist yet
                            if (!relationExists)
                                RelationManager.Instance.AddOrReplace(relation);

                            // Raise event
                            relation.RaiseSeenEvent(location, previousKnow - relation.Know, previousLike - relation.Like);
                        }
                    }
                }
            }
        }
    }
}