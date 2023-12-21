using System.Linq;
using System.Reflection;
using HarmonyLib;
using Rewired;
using SOD.Common.Shadows;
using SOD.Common.Shadows.Implementations;
using UnityEngine;

namespace SOD.Common.Patches
{
    internal class RewiredPlayerPatches
    {
        [HarmonyPatch]
        internal class RewiredPlayer_GetButtonDown
        {
            internal static int lastFrameCount = -1;

            [HarmonyTargetMethod]
            internal static MethodBase CalculateMethod()
            {
                return typeof(Rewired.InputManager_Base).Assembly.ExportedTypes
                    .Single(t => t.FullName == "Rewired.Player")
                    .GetMethods()
                    .Where(m => m.Name == "GetButtonDown")
                    .Single(m => m.GetParameters().Single().ParameterType == typeof(string));
            }

            internal static void Postfix(string actionName, ref bool __result)
            {
                if (!__result)
                {
                    return;
                }

                var currFrameCount = UnityEngine.Time.frameCount;
                if (currFrameCount == lastFrameCount)
                {
                    return;
                }
                lastFrameCount = currFrameCount;
                Lib.InputDetection.OnButtonStateChanged(actionName, true);
            }
        }

        [HarmonyPatch]
        internal class RewiredPlayer_GetButtonUp
        {
            internal static int lastFrameCount = -1;

            [HarmonyTargetMethod]
            internal static MethodBase CalculateMethod()
            {
                return typeof(Rewired.InputManager_Base).Assembly.ExportedTypes
                    .Single(t => t.FullName == "Rewired.Player")
                    .GetMethods()
                    .Where(m => m.Name == "GetButtonUp")
                    .Single(m => m.GetParameters().Single().ParameterType == typeof(string));
            }

            internal static void Postfix(string actionName, ref bool __result)
            {
                if (!__result)
                {
                    return;
                }

                var currFrameCount = UnityEngine.Time.frameCount;
                if (currFrameCount == lastFrameCount)
                {
                    return;
                }
                lastFrameCount = currFrameCount;
                Lib.InputDetection.OnButtonStateChanged(actionName, false);
            }
        }
    }
}