using HarmonyLib;
using SOD.Common.Extensions;
using System;

namespace SOD.ClumsyCitizens
{
    public class PluginHooks
    {
        [HarmonyPatch(typeof(NewDoor), nameof(NewDoor.OnClose))]
        public class NewDoor_OnClose
        {
            private static bool _hasAdjusted;
            private static Interactable _lockInteractableFront, _lockInteractableRear;

            // Lazy loaded class type, incase this patch is never executed
            private static Type _class;
            private static Type Class { get { return _class ??= typeof(NewDoor_OnClose); } }

            [HarmonyPrefix]
            public static void Prefix(Actor actor, NewDoor __instance)
            {
                Plugin.Log.LogMethodEntry(Class, nameof(Prefix));

                // If the door is suppose to be locked behind, have an opportunity to be clumsy
                if (!__instance.isLocked && __instance.preset.armLockOnClose)
                {
                    Plugin.Log.LogInfo($"Door locked state before postfix: " + (__instance.isLocked ? "locked" : "unlocked"));
                    if (Toolbox.Instance.Rand(0, 100, true) < Plugin.Instance.Config.ChanceToForgetDoorLock)
                    {
                        _lockInteractableFront = __instance.lockInteractableFront;
                        _lockInteractableRear = __instance.lockInteractableRear;
                        __instance.lockInteractableFront = null;
                        __instance.lockInteractableRear = null;
                        _hasAdjusted = true;
                        Plugin.Log.LogInfo($"Left door({__instance.GetInstanceID()}) unlocked.");
                    }
                }

                Plugin.Log.LogMethodExit(Class, nameof(Prefix));
            }

            [HarmonyPostfix]
            public static void Postfix(Actor actor, NewDoor __instance)
            {
                Plugin.Log.LogMethodEntry(Class, nameof(Postfix));

                if (_hasAdjusted)
                {
                    // Reset previous state
                    _hasAdjusted = false;
                    __instance.lockInteractableFront = _lockInteractableFront;
                    __instance.lockInteractableRear = _lockInteractableRear;
                    _lockInteractableFront = null;
                    _lockInteractableRear = null;
                    Plugin.Log.LogInfo($"Door locked state after postfix: " + (__instance.isLocked ? "locked" : "unlocked"));
                }

                Plugin.Log.LogMethodExit(Class, nameof(Postfix));
            }
        }
    }
}
