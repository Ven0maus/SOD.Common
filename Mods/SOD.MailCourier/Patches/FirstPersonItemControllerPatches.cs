using HarmonyLib;

namespace SOD.MailCourier.Patches
{
    internal static class FirstPersonItemControllerPatches
    {
        internal static Interactable MailCourierInteractable;

        [HarmonyPatch(typeof(FirstPersonItemController), nameof(FirstPersonItemController.PickUpItem))]
        internal static class FirstPersonItemController_PickUpItem
        {
            [HarmonyPrefix]
            internal static void Prefix(Interactable pickUpThis)
            {
                if (pickUpThis != null && pickUpThis.preset != null &&
                    pickUpThis.preset.summaryMessageSource != null &&
                    pickUpThis.preset.summaryMessageSource == "mail_courier_job_message" &&
                    ShopSelectButtonControllerPatches.ShopSelectButtonController_PurchaseExecute.ComingFromAPurchase)
                {
                    MailCourierInteractable = pickUpThis;
                }
            }
        }
    }
}
