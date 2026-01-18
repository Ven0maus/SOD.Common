using HarmonyLib;
using SOD.MailCourier.Core;

namespace SOD.MailCourier.Patches
{
    // Properly sets the description tooltip of the mail courier job.
    internal static class ShopSelectButtonControllerPatches
    {
        [HarmonyPatch(typeof(ShopSelectButtonController), nameof(ShopSelectButtonController.UpdateTooltip))]
        internal static class ShopSelectButtonController_UpdateTooltip
        {
            [HarmonyPrefix]
            internal static bool Prefix(ShopSelectButtonController __instance)
            {
                if (__instance.preset == null || __instance.preset.summaryMessageSource != "mail_courier_job_message")
                    return true;

                __instance.tooltip.mainText = "Generates a random mail delivery mission, should you choose to accept it.";
                return true;
            }
        }

        [HarmonyPatch(typeof(ShopSelectButtonController), nameof(ShopSelectButtonController.PurchaseExecute))]
        internal static class ShopSelectButtonController_PurchaseExecute
        {
            internal static bool ComingFromAPurchase = false;

            [HarmonyPrefix]
            internal static void Prefix(ShopSelectButtonController __instance, ref int __state)
            {
                if (__instance.preset == null || __instance.preset.summaryMessageSource != "mail_courier_job_message")
                    return;

                // Track money from before the transaction
                __state = GameplayController.Instance.money;
                ComingFromAPurchase = true;
            }

            [HarmonyPostfix]
            internal static void Postfix(ShopSelectButtonController __instance, ref int __state)
            {
                ComingFromAPurchase = false;
                if (__instance.preset == null || __instance.preset.summaryMessageSource != "mail_courier_job_message")
                    return;

                // If money is the same, then something went wrong with the transaction
                if (GameplayController.Instance.money == __state) return;

                if (FirstPersonItemControllerPatches.MailCourierInteractable != null)
                {
                    CourierJobGenerator.CreateJob(FirstPersonItemControllerPatches.MailCourierInteractable.id);
                    FirstPersonItemControllerPatches.MailCourierInteractable = null; // Cleanup again
                }
            }
        }
    }
}
