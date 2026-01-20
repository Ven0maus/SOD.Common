using HarmonyLib;
using SOD.Common;
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

        [HarmonyPatch(typeof(BioScreenController), nameof(BioScreenController.SelectSlot))]
        internal static class BioScreenController_SelectSlot
        {
            [HarmonyPrefix]
            internal static bool Prefix(BioScreenController __instance, FirstPersonItemController.InventorySlot newSlot)
            {
                if (__instance.selectedSlot != null && __instance.isOpen)
                {
                    Interactable sealedMail = __instance.selectedSlot.GetInteractable();
                    if (sealedMail != null && sealedMail.preset.summaryMessageSource != null &&
                        sealedMail.preset.summaryMessageSource == "mail_courier_job_message")
                    {
                        if (newSlot.interactableID == -1)
                        {
                            // We unequipped
                            CourierJobGenerator.SetDestinationRoute(null);
                        }
                        else if (newSlot.interactableID != sealedMail.id)
                        {
                            // Check if we are equiping another sealed mail, then adjust destination route
                            var courierJob = CourierJobGenerator.FindJobBySealedMailId(newSlot.interactableID);
                            if (courierJob != null)
                                CourierJobGenerator.SetDestinationRoute(courierJob.Mailbox);
                            else
                                CourierJobGenerator.SetDestinationRoute(null);
                        }
                    }
                    else
                    {
                        if (newSlot.interactableID != -1)
                        {
                            // Check if we are equiping a sealed mail, then adjust destination route
                            var courierJob = CourierJobGenerator.FindJobBySealedMailId(newSlot.interactableID);
                            if (courierJob != null)
                                CourierJobGenerator.SetDestinationRoute(courierJob.Mailbox);
                            else
                                CourierJobGenerator.SetDestinationRoute(null);
                        }
                    }
                }
                return true;
            }
        }
    }
}
