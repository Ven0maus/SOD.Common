using HarmonyLib;

namespace SOD.RelationsPlus.Patches
{
    internal class NewAIControllerPatches
    {
        [HarmonyPatch(typeof(NewAIController), nameof(NewAIController.TalkTo))]
        internal static class NewAIController_TalkTo
        {
            [HarmonyPrefix]
            internal static void Prefix(ref Interactable __state)
            {
                __state = Player.Instance.interactingWith;
            }

            [HarmonyPostfix]
            internal static void Postfix(NewAIController __instance, ref Interactable __state)
            {
                if (Player.Instance.interactingWith == null || (__state != null && Player.Instance.interactingWith.id == __state.id)) return;

                // Increase know when talking to citizen
                RelationManager.Instance[__instance.human.humanID].Know += Plugin.Instance.Config.SpeakingToCitizenModifier;
            }
        }
    }
}
