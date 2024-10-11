using SOD.Common;
using SOD.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SOD.Narcotics.AddictionCore
{
    public abstract class Addiction : IAddiction
    {
        public string AddictionName => $"{AddictionType} addiction";
        public AddictionType AddictionType { get; }
        public float Progression { get; set; }
        public bool Recovering
        {
            get
            {
                return TimeSinceLastWorsening.AddHours(Plugin.Instance.Config.RecoveryStartTimeAfterHours) <= Lib.Time.CurrentDateTime;
            }
        }
        public AddictionStage Stage { get; set; }
        public Time.TimeData TimeSinceLastWorsening { get; set; }

        public HashSet<AddictionStage> AppliedStageEffects { get; set; } = new();

        public Addiction(AddictionType addictionType)
        {
            AddictionType = addictionType;
            Stage = AddictionStage.Mild;
        }

        /// <summary>
        /// Worsens the addictions
        /// </summary>
        /// <param name="progressAmount"></param>
        public void AdjustProgress(float progressAmount)
        {
            if (progressAmount > 0)
                TimeSinceLastWorsening = Lib.Time.CurrentDateTime;

            Progression += progressAmount;

            if (Progression >= 1f && Stage < AddictionStage.Extreme)
            {
                Progression = 0f; // Reset progress for the next stage
                MoveToNextStage();
            }
            else if (Progression <= 0f)
            {
                Progression = 1f; // Make sure progression starts at the top
                MoveToPreviousStage();
            }

            if (Plugin.Instance.Config.DebugMode)
                Plugin.Log.LogInfo($"[{AddictionName}] [Progress]: {(progressAmount > 0 ? Progression * 100 : 100 - Progression * 100)}% towards {(progressAmount > 0 ? "worsening" : "recovery")}.");
        }

        public void Cure()
        {
            Progression = 0f;

            // Remove all effects
            for (int i=(int)Stage; i >= 0; i--)
            {
                Stage = (AddictionStage)i;
                RemoveStageEffects();
            }

            if (Plugin.Instance.Config.DebugMode)
                Plugin.Log.LogInfo($"You have now been cured of \"{AddictionName}\".");
        }

        public void Initialize()
        {
            // For loading, or creating new
            if (AppliedStageEffects.Count > 0)
            {
                var currentStage = Stage;
                var effects = AppliedStageEffects.ToArray();
                AppliedStageEffects.Clear();
                foreach (var effect in effects)
                {
                    Stage = effect;
                    ApplyStageEffects();
                }
                Stage = currentStage;
            }
            else
            {
                TimeSinceLastWorsening = Lib.Time.CurrentDateTime;
                ApplyStageEffects();
            }
        }

        public abstract Action<bool> MildStageAction();
        public abstract Action<bool> SevereStageAction();
        public abstract Action<bool> ExtremeStageAction();

        private void ApplyStageEffects()
        {
            if (AppliedStageEffects.Contains(Stage)) return;
            GetStageAction()?.Invoke(true);
            AppliedStageEffects.Add(Stage);

            if (Plugin.Instance.Config.DebugMode)
                Plugin.Log.LogInfo($"[{AddictionName}] Applied effects of stage \"{Stage}\".");
        }

        private void RemoveStageEffects()
        {
            if (!AppliedStageEffects.Contains(Stage)) return;
            GetStageAction()?.Invoke(false);
            AppliedStageEffects.Remove(Stage);

            if (Plugin.Instance.Config.DebugMode)
                Plugin.Log.LogInfo($"[{AddictionName}] Removed effects of stage \"{Stage}\".");
        }

        private Action<bool> GetStageAction()
        {
            switch (Stage)
            {
                case AddictionStage.Mild:
                    return MildStageAction();
                case AddictionStage.Severe:
                    return SevereStageAction();
                case AddictionStage.Extreme:
                    return ExtremeStageAction();
            }

            throw new NotSupportedException($"Stage \"{Stage}\" is not supported.");
        }

        private void MoveToNextStage()
        {
            Stage++;
            ApplyStageEffects();

            if (Plugin.Instance.Config.DebugMode)
                Plugin.Log.LogInfo($"[{AddictionName}] [Stage]: Worsened to {Stage}.");
        }

        private void MoveToPreviousStage()
        {
            RemoveStageEffects();

            // When we're already in mild, we're cured
            if (Stage == AddictionStage.Mild)
            {
                AddictionManager.Cure(AddictionType);
                return;
            }

            Stage--;

            if (Plugin.Instance.Config.DebugMode)
                Plugin.Log.LogInfo($"[{AddictionName}] [Stage]: Improved to {Stage}.");
        }
    }
}
