using HarmonyLib;

namespace SOD.RelationsPlus.Patches
{
    internal class SideJobPatches
    {
        [HarmonyPatch(typeof(SideJob), nameof(SideJob.AcceptJob))]
        internal static class SideJob_AcceptJob
        {
            [HarmonyPrefix]
            internal static void Prefix(SideJob __instance, ref bool __state)
            {
                __state = __instance.accepted;
            }

            [HarmonyPostfix]
            internal static void Postfix(SideJob __instance, ref bool __state)
            {
                // Job was accepted, add to know
                if (!__state && __instance.accepted)
                {
                    if (__instance.poster != null && __instance.posterID > 0)
                    {
                        RelationManager.Instance[__instance.posterID].Know += Plugin.Instance.Config.AcceptedJobKnowModifier;
                        RelationManager.Instance[__instance.posterID].Like += Plugin.Instance.Config.AcceptedJobLikeModifier;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Case), nameof(Case.Resolve))]
        internal static class Case_Resolve
        {
            [HarmonyPostfix]
            internal static void Postfix(Case __instance)
            {
                if (__instance.caseType != Case.CaseType.sideJob || __instance.job == null || __instance.job.poster == null) return;

                if (__instance.isSolved)
                    RelationManager.Instance[__instance.job.posterID].Like += Plugin.Instance.Config.SolvedJobModifier;
                else
                    RelationManager.Instance[__instance.job.posterID].Like += Plugin.Instance.Config.FailedJobModifier;
            }
        }
    }
}
