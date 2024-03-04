using HarmonyLib;
using System;

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
                int totalMin = 0, totalMax = 0;
                foreach (var resolveQuestion in __instance.murderResolveQuestions)
                {
                    var min = (int)resolveQuestion.rewardRange.x - (int)(resolveQuestion.rewardRange.x / 100 * reduction);
                    var max = (int)resolveQuestion.rewardRange.y - (int)(resolveQuestion.rewardRange.y / 100 * reduction);
                    resolveQuestion.rewardRange = new UnityEngine.Vector2(min, max);
                    totalMin += min;
                    totalMax += max;
                    count++;
                }

                // Fall-back to make murder reward atleast 50€
                if (totalMin < 50 || totalMax < 50)
                {
                    var totalPerQuestion = (int)Math.Ceiling(50f / __instance.murderResolveQuestions.Count);
                    foreach (var resolveQuestion in __instance.murderResolveQuestions)
                        resolveQuestion.rewardRange = new UnityEngine.Vector2(totalPerQuestion, totalPerQuestion);
                }
                Plugin.Log.LogInfo($"Reduced \"{count}\" murder resolve question payouts.");
            }
        }
    }
}
