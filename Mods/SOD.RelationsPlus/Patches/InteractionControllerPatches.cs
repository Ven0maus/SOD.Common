using HarmonyLib;
using System;
using SOD.Common.Extensions;

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
                    if (actor.isMachine || actor.isPlayer) continue;
                    if (!actor.Sees(Player.Instance)) continue;

                    Human human = null;
                    try
                    {
                        human = ((dynamic)actor).Cast<Human>();
                    }
                    catch(InvalidCastException)
                    { }

                    if (human == null) continue;

                    if (Plugin.Instance.Config.DebugMode)
                        Plugin.Log.LogInfo($"[Debug]: Illegal activity seen by {human.GetCitizenName()}!");

                    RelationManager.Instance[human.humanID].Like += Plugin.Instance.Config.SeenDoingIllegalModifier;
                }
            }
        }
    }
}
