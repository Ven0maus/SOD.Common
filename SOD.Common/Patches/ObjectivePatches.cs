using HarmonyLib;
using SOD.Common.Helpers.ObjectiveObjects;

namespace SOD.Common.Patches
{
    internal class ObjectivePatches
    {
        [HarmonyPatch(typeof(Objective), nameof(Objective.Complete))]
        internal class Objective_Complete
        {
            [HarmonyPostfix]
            private static void Postfix(Objective __instance)
            {
                // Remove the tracking of the custom objective and its resources
                ClearOutCustomObjective(__instance, out var customObjective);

                // Handle on complete actions
                if (customObjective != null)
                {
                    // If action is available, execute it
                    customObjective.OnComplete?.Invoke(__instance);
                    
                    // Execute children objectives
                    if (customObjective.Children != null)
                    {
                        foreach (var child in customObjective.Children)
                        {
                            if (child.Trigger == ObjectiveBuilder.ChildTrigger.OnCompletion)
                                child.Builder.RegisterInternal();
                        }
                    }
                }

                // Raise completed event
                Lib.CaseObjectives.RaiseEvent(Helpers.CaseObjectives.Event.ObjectiveCompleted, __instance, customObjective != null);
            }
        }

        [HarmonyPatch(typeof(Objective), nameof(Objective.Cancel))]
        internal class Objective_Cancel
        {
            [HarmonyPostfix]
            private static void Postfix(Objective __instance)
            {
                // Remove the tracking of the custom objective and its resources
                ClearOutCustomObjective(__instance, out var customObjective);

                // Handle on complete actions
                if (customObjective != null)
                {
                    // If action is available, execute it
                    customObjective.OnCancel?.Invoke(__instance);

                    // Execute children objectives
                    if (customObjective.Children != null)
                    {
                        foreach (var child in customObjective.Children)
                        {
                            if (child.Trigger == ObjectiveBuilder.ChildTrigger.OnCancelation)
                                child.Builder.RegisterInternal();
                        }
                    }
                }

                // Raise completed event
                Lib.CaseObjectives.RaiseEvent(Helpers.CaseObjectives.Event.ObjectiveCanceled, __instance, customObjective != null);
            }
        }

        [HarmonyPatch(typeof(Objective), nameof(Objective.Setup))]
        internal class Objective_Setup
        {
            [HarmonyPostfix]
            private static void Postfix(Objective __instance)
            {
                var customObjective = FindCustomObjective(__instance);

                // Raise completed event
                Lib.CaseObjectives.RaiseEvent(Helpers.CaseObjectives.Event.ObjectiveCreated, __instance, customObjective != null);
            }
        }

        /// <summary>
        /// Attempts to see if a custom objective exists for this objective and returns it.
        /// </summary>
        /// <param name="objective"></param>
        /// <returns></returns>
        private static CustomObjective FindCustomObjective(Objective objective)
        {
            if (Lib.CaseObjectives.CustomObjectives.TryGetValue(objective.queueElement.entryRef, out var customObjective))
                return customObjective;
            return null;
        }

        /// <summary>
        /// Attempts to remove the custom objective if it exists
        /// </summary>
        /// <param name="objective"></param>
        private static void ClearOutCustomObjective(Objective objective, out CustomObjective customObjective)
        {
            customObjective = FindCustomObjective(objective);
            if (customObjective != null)
            {
                // Remove the occupied dds record as its no longer needed
                Lib.DdsStrings[customObjective.DictionaryRef, customObjective.EntryRef] = null;

                // Remove custom objective
                Lib.CaseObjectives.CustomObjectives.Remove(customObjective.EntryRef);
            }
        }
    }
}
