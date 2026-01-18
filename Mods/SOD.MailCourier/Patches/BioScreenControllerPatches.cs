using HarmonyLib;
using SOD.MailCourier.Core;

namespace SOD.MailCourier.Patches
{
    internal static class BioScreenControllerPatches
    {
        [HarmonyPatch(typeof(BioScreenController), nameof(BioScreenController.UpdateSummary))]
        internal static class BioScreenController_UpdateSummary
        {
            [HarmonyPrefix]
            internal static bool Prefix(BioScreenController __instance)
            {
                if (__instance.hoveredSlot != null && __instance.isOpen)
                {
                    Interactable sealedMail = __instance.hoveredSlot.GetInteractable();
                    if (sealedMail != null && sealedMail.preset.summaryMessageSource != null && 
                        sealedMail.preset.summaryMessageSource == "mail_courier_job_message")
                    {
                        var courierJob = CourierJobGenerator.FindJobBySealedMailId(sealedMail.id);
                        if (courierJob != null)
                        {
                            __instance.summaryTextToDisplay = "Delivery address: " + courierJob.DeliveryAddressName + " Mailbox";
                            return false;
                        }
                        else
                        {
                            __instance.summaryTextToDisplay = "This delivery is no longer valid, please discard this item.";
                        }
                    }
                }
                return true;
            }
        }
    }
}
