using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace SOD.RelationsPlus.Patches
{
    internal class InteractionControllerPatches
    {
        [HarmonyPatch(typeof(InteractionController), nameof(InteractionController.GetValidPlayerActionIllegal))]
        internal static class InteractionController_GetValidPlayerActionIllegal
        {
            [HarmonyPostfix]
            internal static void Postfix(Interactable inter, ref bool __result)
            {
                if (inter == null || inter.belongsTo == null || inter.belongsTo.humanID == Player.Instance.humanID) return;
                if (__result)
                {
                    // Are we seen by the person who the interactable belongs to?
                    var belongsTo = inter.belongsTo;
                    float distance = Vector3.Distance(belongsTo.lookAtThisTransform.position, Player.Instance.lookAtThisTransform.position);
                    if (distance < GameplayControls.Instance.minimumStealthDetectionRange ||
                        belongsTo.ActorRaycastCheck(Player.Instance, distance + 3f, out _, false, Color.green, Color.red, Color.white, 1f))
                    {
                        RelationManager.Instance[belongsTo.humanID].Like += Plugin.Instance.Config.SeenStealingModifier;
                    }
                }
            }
        }
    }
}
