using SOD.Common.Helpers.ObjectiveObjects;
using System;
using System.Collections.Generic;

namespace SOD.Common.Helpers
{
    public sealed class CaseObjectives
    {
        internal CaseObjectives() { }

        /// <summary>
        /// Contains all the custom objectives
        /// </summary>
        internal Dictionary<string, CustomObjective> CustomObjectives { get; } = new Dictionary<string, CustomObjective>();

        /// <summary>
        /// Raised when a new objective is created.
        /// <br>Note: It also raises on custom objectives see "<see cref="ObjectiveArgs.IsCustom"/>".</br>
        /// </summary>
        public event EventHandler<ObjectiveArgs> OnObjectiveCreated;

        /// <summary>
        /// Raised when an objective is completed.
        /// <br>Note: It also raises on custom objectives see "<see cref="ObjectiveArgs.IsCustom"/>".</br>
        /// </summary>
        public event EventHandler<ObjectiveArgs> OnObjectiveCompleted;

        /// <summary>
        /// Raised when an objective is canceled.
        /// <br>Note: It also raises on custom objectives see "<see cref="ObjectiveArgs.IsCustom"/>".</br>
        /// </summary>
        public event EventHandler<ObjectiveArgs> OnObjectiveCanceled;

        /// <summary>
        /// Return's a basic objective builder object, to help you build a new objective.
        /// <br>Call <see cref="ObjectiveBuilder.Register"/> to create the objective.</br>
        /// </summary>
        /// <param name="case">The case the objective should be a part of.</param>
        /// <returns></returns>
        public ObjectiveBuilder Builder(Case @case)
        {
            return new ObjectiveBuilder(@case);
        }

        internal void RaiseEvent(Event @event, Objective objective, bool isCustom)
        {
            switch(@event)
            {
                case Event.ObjectiveCreated:
                    if (Plugin.InDebugMode && isCustom)
                        Plugin.Log.LogInfo($"[DebugMode]: Objective Created: {objective.queueElement?.entryRef}");
                    OnObjectiveCreated?.Invoke(this, new ObjectiveArgs(objective, isCustom));
                    break;
                case Event.ObjectiveCompleted:
                    if (Plugin.InDebugMode && isCustom)
                        Plugin.Log.LogInfo($"[DebugMode]: Objective Completed: {objective.queueElement?.entryRef}");
                    OnObjectiveCompleted?.Invoke(this, new ObjectiveArgs(objective, isCustom));
                    break;
                case Event.ObjectiveCanceled:
                    if (Plugin.InDebugMode && isCustom)
                        Plugin.Log.LogInfo($"[DebugMode]: Objective Canceled: {objective.queueElement?.entryRef}");
                    OnObjectiveCanceled?.Invoke(this, new ObjectiveArgs(objective, isCustom));
                    break;
            }
        }

        internal enum Event
        {
            ObjectiveCreated,
            ObjectiveCompleted,
            ObjectiveCanceled,
        }
    }
}
