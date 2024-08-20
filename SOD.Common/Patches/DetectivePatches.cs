using HarmonyLib;

namespace SOD.Common.Patches
{
    internal class DetectivePatches
    {
        [HarmonyPatch(typeof(Human.Death), nameof(Human.Death.SetReported))]
        internal class Death_SetReported
        {
            private static bool _notReported = false;

            [HarmonyPrefix]
            internal static void Prefix(Human.Death __instance)
            {
                if (!__instance.reported)
                {
                    _notReported = true;
                }
            }

            [HarmonyPostfix]
            internal static void Postfix(Human.Death __instance, Human newFoundBy, Human.Death.ReportType newReportType)
            {
                if (_notReported)
                {
                    _notReported = false;

                    // Raise method for victim discovery
                    var victim = __instance.GetVictim();
                    if (victim != null)
                        Lib.Gameplay.ReportVictim(victim, newFoundBy, newReportType);
                }
            }
        }
    }
}
