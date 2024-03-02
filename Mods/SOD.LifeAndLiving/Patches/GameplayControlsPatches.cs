using HarmonyLib;

namespace SOD.LifeAndLiving.Patches
{
    internal class GameplayControlsPatches
    {
        [HarmonyPatch(typeof(GameplayControls), nameof(GameplayControls.Awake))]
        internal class GameplayControls_Awake
        {
            [HarmonyPostfix]
            internal static void Postfix(GameplayControls __instance)
            {
                ReduceMurderPayouts(__instance);
            }

            private static void ReduceMurderPayouts(GameplayControls __instance)
            {
                var reduction = Plugin.Instance.Config.PayoutReductionMurders;
                int count = 0;
                foreach (var resolveQuestion in __instance.murderResolveQuestions)
                {
                    var min = resolveQuestion.rewardRange.x - (resolveQuestion.rewardRange.x / 100 * reduction);
                    var max = resolveQuestion.rewardRange.y - (resolveQuestion.rewardRange.y / 100 * reduction);
                    resolveQuestion.rewardRange = new UnityEngine.Vector2(min, max);
                    count++;
                }
                Plugin.Log.LogInfo($"Reduced \"{count}\" murder resolve question payouts.");
            }
        }
    }
}
