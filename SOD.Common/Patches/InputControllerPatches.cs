using HarmonyLib;
using Rewired;
using SOD.Common.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace SOD.Common.Patches
{
    internal class InputControllerPatches
    {
        private static readonly List<string> _actionNames = new();
        private static readonly Dictionary<string, InteractablePreset.InteractionKey> _gameMappedKeyDictionary = new();

        private static void InitializeActionsIfNecessary()
        {
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
                var gameMappedKeys = System.Enum.GetNames(typeof(InteractablePreset.InteractionKey));
                foreach (var key in gameMappedKeys)
                {
                    var capitalizedKey = $"{key.Substring(0, 1).ToUpper()}{key.Substring(1)}";
                    if (!_actionNames.Contains(capitalizedKey))
                    {
                        continue;
                    }
                    _gameMappedKeyDictionary.Add(capitalizedKey, System.Enum.Parse<InteractablePreset.InteractionKey>(key, true));
                }
            }
        }

        private static void ReportIfSuppressed(string actionName, ref bool __result, bool isDown)
        {
            var interactionKey = _gameMappedKeyDictionary[actionName];

            var action = ReInput.MappingHelper.Instance.GetAction(actionName);
            if (ReInput.MappingHelper.Instance.GetActionElementMap(action._id)._elementType != ControllerElementType.Button)
            {
                return;
            }
            var boundKeyCode = ReInput.MappingHelper.Instance.GetActionElementMap(action._id).keyCode;

            foreach (var (key, value) in Lib.InputDetection.InputSuppressionDictionary)
            {
                if (Lib.InputDetection.InputSuppressionDictionary.Values.Any(v => v.InteractionKey == interactionKey || v.KeyCode == boundKeyCode))
                {
                    // Report the state change and that it was suppressed
                    Lib.InputDetection.ReportButtonStateChange(actionName, interactionKey, isDown: isDown, isSuppressed: true);
                    __result = false;
                }
            }
        }

        [HarmonyPatch(typeof(Rewired.Player), nameof(Rewired.Player.GetButtonDown), argumentTypes: [typeof(string)])]
        internal class Rewired_Player_GetButtonDown
        {
            [HarmonyPostfix]
            internal static void Postfix(Rewired.Player __instance, string actionName, ref bool __result)
            {
                if (!ReInput.isReady || !__result || __instance == null)
                {
                    return;
                }
                InitializeActionsIfNecessary();
                ReportIfSuppressed(actionName, ref __result, isDown: true);
            }

        }

        [HarmonyPatch(typeof(Rewired.Player), nameof(Rewired.Player.GetButtonUp), argumentTypes: [typeof(string)])]
        internal class Rewired_Player_GetButtonUp
        {
            [HarmonyPostfix]
            internal static void Postfix(Rewired.Player __instance, string actionName, ref bool __result)
            {
                if (!ReInput.isReady || !__result || __instance == null)
                {
                    return;
                }
                InitializeActionsIfNecessary();
                ReportIfSuppressed(actionName, ref __result, isDown: false);
            }
        }

        /// <summary>
        /// Patch for game input actions which use one of GetButton,
        /// GetButtonDown, GetButtonUp instead of GetButtonDown and
        /// GetButtonUp.
        /// </summary>
        [HarmonyPatch(typeof(InputController), nameof(InputController.Update))]
        internal class InputController_Update
        {
            [HarmonyPrefix]
            internal static void Prefix(InputController __instance)
            {
                if (!__instance.enabled || !ReInput.isReady || __instance.player == null)
                {
                    return;
                }
                InitializeActionsIfNecessary();
                if (!__instance.player.GetAnyButtonDown() && !__instance.player.GetAnyButtonUp())
                {
                    return;
                }
                foreach (var actionName in _actionNames)
                {
                    bool isBtnDown = __instance.player.GetButtonDown(actionName);
                    if (isBtnDown || __instance.player.GetButtonUp(actionName))
                    {
                        var interactionKey = _gameMappedKeyDictionary.ContainsKey(actionName)
                            ? _gameMappedKeyDictionary[actionName]
                            : InteractablePreset.InteractionKey.none;

                        Lib.InputDetection.ReportButtonStateChange(actionName, interactionKey, isBtnDown, false);
                    }
                }
            }
        }
    }
}
