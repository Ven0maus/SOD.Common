using HarmonyLib;
using System;

namespace SOD.LifeAndLiving.Content.EconomyRebalance
{
    internal class GameplayControlsPatches
    {
        [HarmonyPatch(typeof(GameplayControls), nameof(GameplayControls.Awake))]
        internal class GameplayControls_Awake
        {
            [HarmonyPostfix]
            internal static void Postfix(GameplayControls __instance)
            {
                if (Plugin.Instance.Config.DisableEconomyRebalance) return;

                ReduceMurderPayouts(__instance);
            }

            private static void ReduceMurderPayouts(GameplayControls __instance)
            {
                var reduction = Plugin.Instance.Config.PayoutReductionMurders;
                var minPayout = Plugin.Instance.Config.MinimumMurderResolveQuestionPayout;
                int count = 0;
                foreach (var resolveQuestion in __instance.murderResolveQuestions)
                {
                    var hashcode = resolveQuestion.GetHashCode();
                    var range = Plugin.GetCalculatedRange("murder_payout_reward", hashcode, () =>
                    {
                        var min = Math.Max(minPayout, (int)resolveQuestion.rewardRange.x - (int)(resolveQuestion.rewardRange.x / 100 * reduction));
                        var max = Math.Max(minPayout, (int)resolveQuestion.rewardRange.y - (int)(resolveQuestion.rewardRange.y / 100 * reduction));

                        if (Plugin.Instance.Config.DebugMode)
                            Plugin.Log.LogInfo($"[<color={ConsoleColor.Cyan}>DEBUGMODE</color>]: Init murder resolve question price {resolveQuestion.name}: ({min}, {max})");

                        return new UnityEngine.Vector2(min, max);
                    });

                    // Reduce reward
                    resolveQuestion.rewardRange = range;

                    // Increase penalty by 1/4 the reduction if applicable
                    if (resolveQuestion.penaltyRange.x > 0 && resolveQuestion.penaltyRange.y > 0)
                    {
                        range = Plugin.GetCalculatedRange("murder_payout_penalty", hashcode, () =>
                        {
                            var min = Math.Max(minPayout, (int)resolveQuestion.penaltyRange.x + (int)(resolveQuestion.penaltyRange.x / 100 * (reduction / 4)));
                            var max = Math.Max(minPayout, (int)resolveQuestion.penaltyRange.y + (int)(resolveQuestion.penaltyRange.y / 100 * (reduction / 4)));

                            if (Plugin.Instance.Config.DebugMode)
                                Plugin.Log.LogInfo($"[<color={ConsoleColor.Cyan}>DEBUGMODE</color>]: Init murder resolve question penalty price {resolveQuestion.name}: ({min}, {max})");

                            return new UnityEngine.Vector2(min, max);
                        });

                        resolveQuestion.penaltyRange = range;
                    }
                    count++;
                }

                Plugin.Log.LogInfo($"Reduced \"{count}\" murder resolve question payouts.");
            }
        }
    }
}
