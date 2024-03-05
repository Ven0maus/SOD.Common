using HarmonyLib;
using UnityEngine;

namespace SOD.QoL.Patches
{
    internal class ControlDetectControllerPatches
    {
        [HarmonyPatch(typeof(ControlDetectController), nameof(ControlDetectController.Start))]
        internal class ControlDetectController_Start
        {
            // Skips the "press any key to start" screen on game start

            [HarmonyPrefix]
            private static void Prefix(ControlDetectController __instance)
            {
                if (!Plugin.Instance.Config.SkipPressAnyKeyScreenIfNotUsingJoysticks) return;

                // If a joystick is connected, we cannot skip the screen because we need to know what is accessing.
                var joystickNames = Input.GetJoystickNames();
                if (joystickNames != null && joystickNames.Length > 0)
                {
                    __instance.loadSceneTriggered = false;
                    return;
                }

                PlayerPrefs.SetInt("controlMethod", 1);
                __instance.loadSceneTriggered = true;
            }
        }
    }
}
