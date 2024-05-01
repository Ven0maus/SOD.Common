using HarmonyLib;
using System;
using System.Linq;
using UnityEngine;

namespace SOD.RelationsPlus.Patches
{
    internal class InteractionControllerPatches
    {
        [HarmonyPatch(typeof(InteractionController), nameof(InteractionController.SetIllegalActionActive))]
        internal static class InteractionController_SetIllegalActionActive
        {
            [HarmonyPostfix]
            internal static void Prefix(bool val)
            {
                if (!val) return;

                // Needs to run everytime, even if the value didn't change
                foreach (var actor in CityData.Instance.visibleActors)
                {
                    try
                    {
                        var human = ((dynamic)actor).Cast<Human>();
                        if (human == null || actor.isPlayer ||
                            !actor.isSeenByOthers || actor.isDead || actor.isStunned || actor.isAsleep)
                            continue;

                        float distance = Vector3.Distance(human.lookAtThisTransform.position, Player.Instance.lookAtThisTransform.position);
                        float maxDistance = Mathf.Min(GameplayControls.Instance.citizenSightRange, human.stealthDistance);
                        if (distance <= maxDistance)
                        {
                            if (distance < GameplayControls.Instance.minimumStealthDetectionRange ||
                                human.ActorRaycastCheck(Player.Instance, distance + 3f, out RaycastHit _, false, Color.green, Color.red, Color.white, 1f))
                            {
                                if (Plugin.Instance.Config.DebugMode)
                                    Plugin.Log.LogInfo($"Illegal activity seen by {human.GetCitizenName()}!");
                                RelationManager.Instance[human.humanID].Like += Plugin.Instance.Config.SeenDoingIllegalModifier;
                            }
                        }
                    }
                    catch(InvalidCastException)
                    { }
                }
            }
        }
    }
}
