using System.Collections.Generic;
using HarmonyLib;
using Rewired;
using SOD.Common.Extensions;

namespace SOD.Common.Patches
{
    internal class InputControllerPatches
    {
        /// <summary>
        /// Patch for game input actions which use one of GetButton,
        /// GetButtonDown, GetButtonUp instead of GetButtonDown and
        /// GetButtonUp.
        /// </summary>
        [HarmonyPatch(typeof(InputController), nameof(InputController.Update))]
        internal class InputController_Update
        {
            // TODO: Also add actionIds, which Rewired docs say makes polling faster.
            private static List<string> actionNames = new();

            [HarmonyPrefix]
            internal static void Prefix(InputController __instance)
            {
                if (!__instance.enabled || !ReInput.isReady || __instance.player == null)
                {
                    return;
                }
                if (actionNames.Count == 0)
                {
                    var actionList = ReInput.MappingHelper.Instance.Actions.ToList();
                    foreach (var action in actionList)
                    {
                        actionNames.Add(action.name);
                    }
                }
                foreach (var actionName in actionNames)
                {
                    if (__instance.player.GetButtonDown(actionName))
                    {
                        Lib.InputDetection.ReportButtonStateChange(actionName, true);
                    }
                    else if (__instance.player.GetButtonUp(actionName))
                    {
                        Lib.InputDetection.ReportButtonStateChange(actionName, false);
                    }
                }
            }
        }
    }
}