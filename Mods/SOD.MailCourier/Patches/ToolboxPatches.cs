using HarmonyLib;
using SOD.MailCourier.Core;

namespace SOD.MailCourier.Patches
{
    internal static class ToolboxPatches
    {
        [HarmonyPatch(typeof(Toolbox), nameof(Toolbox.LoadAll))]
        internal static class Toolbox_LoadAll
        {
            [HarmonyPostfix]
            internal static void Postfix()
            {
                CourierJobGenerator.RegisterMailCourierJobs();
            }
        }
    }
}
