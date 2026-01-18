using HarmonyLib;
using SOD.MailCourier.Core;
using System.Linq;

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

        [HarmonyPatch(typeof(FirstPersonItemController), nameof(FirstPersonItemController.UpdateCurrentActions))]
        internal static class FirstPersonItemController_UpdateCurrentActions
        {
            [HarmonyPrefix]
            internal static bool Prefix(FirstPersonItemController __instance)
            {
                if (InteractionController.Instance.lookingAtInteractable)
                {
                    if (InteractionController.Instance.currentLookingAtInteractable != null &&
                        InteractionController.Instance.currentLookingAtInteractable.interactable != null)
                    {
                        if (BioScreenController.Instance.selectedSlot != null && BioScreenController.Instance.selectedSlot.interactableID > -1)
                        {
                            var courierJobs = CourierJobGenerator.FindJobsByMailboxId(InteractionController.Instance.currentLookingAtInteractable.interactable.id);
                            if (courierJobs.Any(job => job.SealedMailId == BioScreenController.Instance.selectedSlot.interactableID))
                            {
                                var primaryAction = __instance.currentActions[InteractablePreset.InteractionKey.primary];
                                if (primaryAction.currentAction?.interactionName != "mail_courier_job_message" ||
                                    __instance.currentActions[InteractablePreset.InteractionKey.primary].enabled == false)
                                {
                                    // Disable all other actions
                                    foreach (var action in __instance.currentActions.Values)
                                    {
                                        action.display = false;
                                        action.enabled = false;
                                    }

                                    // Enable our action
                                    var interactableCurrentAction = __instance.currentActions[InteractablePreset.InteractionKey.primary];
                                    interactableCurrentAction.display = true;
                                    interactableCurrentAction.enabled = true;
                                    interactableCurrentAction.currentAction = CourierJobGenerator.InsertMailInteractionAction;
                                }
                                return false;
                            }
                        }
                    }
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(FirstPersonItemController), nameof(FirstPersonItemController.OnInteraction))]
        internal static class FirstPersonItemController_OnInteraction
        {
            [HarmonyPrefix]
            internal static bool Prefix(FirstPersonItemController __instance, InteractablePreset.InteractionKey input)
            {
                if (__instance.currentActions.TryGetValue(input, out var interactableCurrentAction) &&
                    interactableCurrentAction.currentAction?.interactionName == "mail_courier_job_message" &&
                    interactableCurrentAction.enabled && interactableCurrentAction.display)
                {
                    InsertMailIntoMailbox();
                    return false;
                }
                return true;
            }

            private static void InsertMailIntoMailbox()
            {
                if (BioScreenController.Instance.selectedSlot != null && BioScreenController.Instance.selectedSlot.interactableID > -1)
                {
                    var courierJob = CourierJobGenerator.FindJobBySealedMailId(BioScreenController.Instance.selectedSlot.interactableID);
                    if (courierJob != null)
                    {
                        // Destroy job data + add payment
                        CourierJobGenerator.DestroyCourierJob(courierJob);
                        GameplayController.Instance.AddMoney(courierJob.Payment, true, null);
                    }
                }
            }
        }
    }
}