using HarmonyLib;

namespace SOD.Common.Patches
{
    internal class ObjectivePatches
    {
        [HarmonyPatch(typeof(Objective), nameof(Objective.Complete))]
        internal class Objective_Complete
        {
            [HarmonyPostfix]
            private static void Postfix(Objective __instance)
            {

            }
        }

        [HarmonyPatch(typeof(Objective), nameof(Objective.Cancel))]
        internal class Objective_Cancel
        {
            [HarmonyPostfix]
            private static void Postfix(Objective __instance)
            {

            }
        }

        [HarmonyPatch(typeof(Objective), nameof(Objective.Setup))]
        internal class Objective_Setup
        {
            [HarmonyPostfix]
            private static void Postfix(Objective __instance)
            {

            }
        }
    }
}
