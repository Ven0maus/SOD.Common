using HarmonyLib;
using System;

namespace SOD.LifeAndLiving.Patches.EconomyRebalancePatches
{
    internal class GameplayControlsPatches
    {
        [HarmonyPatch(typeof(GameplayControls), nameof(GameplayControls.Awake))]
        internal class GameplayControls_Awake
        {
            private static bool _appliedOnce = false;

            [HarmonyPostfix]
            internal static void Postfix(GameplayControls __instance)
            {
                if (_appliedOnce) return;
                _appliedOnce = true;

                ReduceMurderPayouts(__instance);
            }

            private static void ReduceMurderPayouts(GameplayControls __instance)
            {
                var reduction = Plugin.Instance.Config.PayoutReductionMurders;
                var minPayout = Plugin.Instance.Config.MinimumMurderResolveQuestionPayout;
                int count = 0;
                foreach (var resolveQuestion in __instance.murderResolveQuestions)
                {
                    // Reduce reward
                    var min = Math.Max(minPayout, (int)resolveQuestion.rewardRange.x - (int)(resolveQuestion.rewardRange.x / 100 * reduction));
                    var max = Math.Max(minPayout, (int)resolveQuestion.rewardRange.y - (int)(resolveQuestion.rewardRange.y / 100 * reduction));
                    resolveQuestion.rewardRange = new UnityEngine.Vector2(min, max);

                    // Increase penalty by 1/4 the reduction if applicable
                    if (resolveQuestion.penaltyRange.x > 0 && resolveQuestion.penaltyRange.y > 0)
                    {
                        min = Math.Max(minPayout, (int)resolveQuestion.penaltyRange.x + (int)(resolveQuestion.penaltyRange.x / 100 * (reduction / 4)));
                        max = Math.Max(minPayout, (int)resolveQuestion.penaltyRange.y + (int)(resolveQuestion.penaltyRange.y / 100 * (reduction / 4)));
                        resolveQuestion.penaltyRange = new UnityEngine.Vector2(min, max);
                    }
                    count++;
                }

                Plugin.Log.LogInfo($"Reduced \"{count}\" murder resolve question payouts.");
            }
        }
    }
}
