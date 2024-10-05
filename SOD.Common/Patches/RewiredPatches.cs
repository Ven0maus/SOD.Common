using HarmonyLib;
using Rewired;
using SOD.Common.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace SOD.Common.Patches
{
    internal class RewiredPatches
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
                var gameMappedKeys = System.Enum.GetValues<InteractablePreset.InteractionKey>();
                foreach (var key in gameMappedKeys)
                {
                    var actionName = Lib.InputDetection.GetRewiredActionName(key);
                    if (!_actionNames.Contains(actionName))
                    {
                        continue;
                    }
                    _gameMappedKeyDictionary.Add(actionName, key);
                }
            }
        }

        private static InteractablePreset.InteractionKey GetInteractionKey(string actionName)
        {
            if (!_gameMappedKeyDictionary.TryGetValue(actionName, out var interactionKey))
            {
                return InteractablePreset.InteractionKey.none;
            }
            return interactionKey;
        }

        [HarmonyPatch(typeof(Rewired.Player), nameof(Rewired.Player.GetAxis), argumentTypes: [typeof(string)])]
        internal class Rewired_Player_GetAxis
        {
            [HarmonyPostfix]
            internal static void Postfix(Rewired.Player __instance, string actionName, ref float __result)
            {
                if (!ReInput.isReady)
                {
                    return;
                }
                InitializeActionsIfNecessary();
                var interactionKey = GetInteractionKey(actionName);
                if (!Lib.InputDetection.IsInputSuppressedByAnyPlugin(interactionKey, true, out var entryIds, out _))
                {
                    Lib.InputDetection.ReportAxisStateChange(actionName, interactionKey, __result, entryIds);
                    return;
                }
                Lib.InputDetection.ReportAxisStateChange(actionName, interactionKey, __result, entryIds);
                __result = 0.0f;
            }
        }

        [HarmonyPatch]
        internal class Rewired_Player_GetButtonAndGetButtonDown
        {
            [HarmonyTargetMethods]
            internal static IEnumerable<System.Reflection.MethodBase> TargetMethods()
            {
                var targets = typeof(Rewired.Player).GetMethods().Where(mi => (mi.Name == "GetButton" || mi.Name == "GetButtonDown") && mi.GetParameters().First().ParameterType == typeof(string)).ToList();
                return targets;
            }

            [HarmonyPostfix]
            internal static void Postfix(Rewired.Player __instance, string actionName, ref bool __result)
            {
                if (!ReInput.isReady || !__result || __instance == null)
                {
                    return;
                }
                InitializeActionsIfNecessary();
                var interactionKey = GetInteractionKey(actionName);
                if (!Lib.InputDetection.IsInputSuppressedByAnyPlugin(interactionKey, true, out var entryIds, out _))
                {
                    Lib.InputDetection.ReportButtonStateChange(actionName, interactionKey, true, entryIds);
                    return;
                }
                __result = false;
                Lib.InputDetection.ReportButtonStateChange(actionName, interactionKey, true, entryIds);
            }
        }

        // [HarmonyPatch(typeof(Rewired.Player), nameof(Rewired.Player.GetButtonDown), argumentTypes: [typeof(string)])]
        // internal class Rewired_Player_GetButtonDown
        // {
        //     [HarmonyPostfix]
        //     internal static void Postfix(Rewired.Player __instance, string actionName, ref bool __result)
        //     {
        //         if (!ReInput.isReady)
        //         {
        //             return;
        //         }
        //         InitializeActionsIfNecessary();
        //         var interactionKey = GetInteractionKey(actionName);
        //         if (!Lib.InputDetection.IsInputSuppressed(interactionKey, true, out var entryIds, out _))
        //         {
        //             Lib.InputDetection.ReportButtonStateChange(actionName, interactionKey, true, false);
        //             return;
        //         }
        //         __result = false;
        //         Lib.InputDetection.ReportButtonStateChange(actionName, interactionKey, true, true);
        //     }
        // }

        // [HarmonyPatch(typeof(Rewired.Player), nameof(Rewired.Player.GetButton), argumentTypes: [typeof(string)])]
        // internal class Rewired_Player_GetButton
        // {
        //     [HarmonyPostfix]
        //     internal static void Postfix(Rewired.Player __instance, string actionName, ref bool __result)
        //     {
        //         if (!ReInput.isReady)
        //         {
        //             return;
        //         }
        //         InitializeActionsIfNecessary();
        //         var interactionKey = GetInteractionKey(actionName);
        //         if (!Lib.InputDetection.IsInputSuppressed(interactionKey, true, out _))
        //         {
        //             Lib.InputDetection.ReportButtonStateChange(actionName, interactionKey, true, false);
        //             return;
        //         }
        //         __result = false;
        //         Lib.InputDetection.ReportButtonStateChange(actionName, interactionKey, true, true);
        //     }
        // }

        [HarmonyPatch(typeof(Rewired.Player), nameof(Rewired.Player.GetButtonUp), argumentTypes: [typeof(string)])]
        internal class Rewired_Player_GetButtonUp
        {
            [HarmonyPostfix]
            internal static void Postfix(Rewired.Player __instance, string actionName, ref bool __result)
            {
                if (!ReInput.isReady)
                {
                    return;
                }
                InitializeActionsIfNecessary();
                var interactionKey = GetInteractionKey(actionName);
                if (!Lib.InputDetection.IsInputSuppressedByAnyPlugin(interactionKey, true, out var entryIds, out _))
                {
                    Lib.InputDetection.ReportButtonStateChange(actionName, interactionKey, false, entryIds);
                    return;
                }
                __result = false;
                Lib.InputDetection.ReportButtonStateChange(actionName, interactionKey, false, entryIds);
            }
        }
    }
}
