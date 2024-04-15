using HarmonyLib;
using UnityEngine;

namespace SOD.LifeAndLiving.Patches.EconomyRebalancePatches
{
    internal class PriceVisualPatches
    {
        [HarmonyPatch(typeof(MurderController), nameof(MurderController.UpdateResolveQuestions))]
        internal class MurderController_UpdateResolveQuestions
        {
            [HarmonyPostfix]
            internal static void Postfix(MurderController __instance)
            {
                if (__instance.currentActiveCase == null) return;

                // This will remove the 50 incrementals from reward and penalty
                foreach (var resolveQuestion in __instance.currentActiveCase.resolveQuestions)
                {
                    resolveQuestion.reward = Mathf.RoundToInt(Toolbox.Instance.VectorToRandom(resolveQuestion.rewardRange) * Game.Instance.jobRewardMultiplier);
                    resolveQuestion.penalty = Mathf.RoundToInt(Toolbox.Instance.VectorToRandom(resolveQuestion.penaltyRange) * Game.Instance.jobPenaltyMultiplier);
                }
            }
        }

        [HarmonyPatch(typeof(SideJob), nameof(SideJob.GenerateResolveQuestions))]
        internal class SideJob_GenerateResolveQuestions
        {
            [HarmonyPostfix]
            internal static void Postfix(SideJob __instance)
            {
                // This will remove the 50 incrementals from reward and penalty
                __instance.reward = 0;
                foreach (var resolveQuestion in __instance.resolveQuestions)
                {
                    if (__instance.rewardSyncDisk != null && __instance.rewardSyncDisk.Length > 0)
                        continue;

                    float num = 1f;
                    if (resolveQuestion.inputType == Case.InputType.revengeObjective)
                    {
                        RevengeObjective revengeObjective = __instance.GetRevengeObjective(resolveQuestion);
                        if (revengeObjective != null)
                        {
                            float t = Toolbox.Instance.Rand(0f, 1f, false);
                            num = Mathf.Lerp(revengeObjective.rewardMultiplier.x, revengeObjective.rewardMultiplier.y, t);
                        }
                    }

                    // Update the resolveQuestion reward
                    resolveQuestion.reward = Mathf.RoundToInt(Toolbox.Instance.VectorToRandom(resolveQuestion.rewardRange) * Game.Instance.jobRewardMultiplier * num * GameplayControls.Instance.sideJobDifficultyRewardMultiplier.Evaluate(__instance.GetDifficulty()));
                    resolveQuestion.reward = Mathf.RoundToInt(resolveQuestion.reward * (1f + UpgradeEffectController.Instance.GetUpgradeEffect(SyncDiskPreset.Effect.sideJobPayModifier)));

                    // Update the resolveQuestion penalty
                    resolveQuestion.penalty = Mathf.RoundToInt(Toolbox.Instance.VectorToRandom(resolveQuestion.penaltyRange) * Game.Instance.jobPenaltyMultiplier * GameplayControls.Instance.sideJobDifficultyRewardMultiplier.Evaluate(__instance.GetDifficulty()));

                    // Update the side job reward properly
                    __instance.reward += resolveQuestion.reward;
                }
            }
        }
    }
}
