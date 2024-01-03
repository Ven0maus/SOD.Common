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
                __instance.loadSceneTriggered = true;

                // Set control method to mouse&keyboard or joystick
                var joystickNames = Input.GetJoystickNames();
                PlayerPrefs.SetInt("controlMethod", (joystickNames == null || joystickNames.Length == 0) ? 1 : 0);
            }
        }
    }
}
