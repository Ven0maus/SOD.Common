using HarmonyLib;

namespace SOD.QoL.Patches
{
    internal class ActionControllerPatches
    {
        [HarmonyPatch(typeof(ActionController), nameof(ActionController.Say))]
        internal class ActionController_Say
        {
            // Add functionality to skip conversation

            [HarmonyPostfix]
            private static void Postfix(Interactable what, Actor who)
            {
                if (!Plugin.Instance.Config.EnableSkipConversationPatch) return;

                if (InteractionController.Instance.dialogMode && InteractionController.Instance.dialogOptions.Count == 0)
                {
                    if (who != null && who.TryCast<Human>().humanID == Player.Instance.humanID)
                    {
                        if (DialogController.Instance.cit != null && DialogController.Instance.cit.speechController.speechActive)
                        {
                            if (DialogController.Instance.cit.speechController.activeSpeechBubble != null)
                                UnityEngine.Object.Destroy(DialogController.Instance.cit.speechController.activeSpeechBubble.gameObject);
                        }
                    }
                }
            }
        }
    }
}
