using HarmonyLib;
using Rewired;
using SOD.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

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
            private static readonly List<string> _actionNames = new();
            private static readonly Dictionary<string, InteractablePreset.InteractionKey> _gameMappedKeyDictionary = new();

            [HarmonyPrefix]
            internal static void Prefix(InputController __instance)
            {
                if (!__instance.enabled || !ReInput.isReady || __instance.player == null)
                {
                    return;
                }
                if (_actionNames.Count == 0)
                {
                    var actionList = ReInput.MappingHelper.Instance.Actions.ToList();
                    foreach (var action in actionList)
                    {
                        _actionNames.Add(action.name);
                    }
                }
                if (_gameMappedKeyDictionary.Count == 0)
                {
                    var gameMappedKeys = Enum.GetNames(typeof(InteractablePreset.InteractionKey));
                    foreach (var key in gameMappedKeys)
                    {
                        var capitalizedKey = $"{key.Substring(0, 1).ToUpper()}{key.Substring(1)}";
                        if (!_actionNames.Contains(capitalizedKey))
                        {
                            continue;
                        }
                        _gameMappedKeyDictionary.Add(capitalizedKey, Enum.Parse<InteractablePreset.InteractionKey>(key, true));
                    }
                }
                foreach (var actionName in _actionNames)
                {
                    var key = _gameMappedKeyDictionary.ContainsKey(actionName) ? _gameMappedKeyDictionary[actionName] : InteractablePreset.InteractionKey.none;
                    if (__instance.player.GetButtonDown(actionName))
                    {
                        Lib.InputDetection.ReportButtonStateChange(actionName, key, true);
                    }
                    else if (__instance.player.GetButtonUp(actionName))
                    {
                        Lib.InputDetection.ReportButtonStateChange(actionName, key, false);
                    }
                }
            }
        }
    }
}