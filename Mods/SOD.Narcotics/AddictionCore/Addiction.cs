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
                return TimeSinceLastWorsening.AddHours(Plugin.Instance.Config.RecoveryStartTime) <= Lib.Time.CurrentDateTime;
            }
        }
        public AddictionStage Stage { get; set; }
        public int HumanId { get; }
        public Time.TimeData TimeSinceLastWorsening { get; set; }

        public HashSet<AddictionStage> AppliedStageEffects { get; set; } = new();

        public Addiction(int humanId, AddictionType addictionType)
        {
            HumanId = humanId;
            AddictionType = addictionType;
            Stage = AddictionStage.Mild;
            TimeSinceLastWorsening = Lib.Time.CurrentDateTime;
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
                Progression = 0f; // Reset progress for the previous stage
                MoveToPreviousStage();
            }

            if (Plugin.Instance.Config.DebugMode)
                Plugin.Log.LogInfo($"[Human:{HumanId}] [{AddictionName}] [Progress]: {Progression * 100}% towards {(progressAmount > 0 ? "worsening" : "recovery")}.");
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
                Plugin.Log.LogInfo($"[Human:{HumanId}] has now been cured of his \"{AddictionName}\".");
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
                Plugin.Log.LogInfo($"[Human:{HumanId}] [{AddictionName}] Applied effects of stage \"{Stage}\".");
        }

        private void RemoveStageEffects()
        {
            if (!AppliedStageEffects.Contains(Stage)) return;
            GetStageAction()?.Invoke(false);
            AppliedStageEffects.Remove(Stage);

            if (Plugin.Instance.Config.DebugMode)
                Plugin.Log.LogInfo($"[Human:{HumanId}] [{AddictionName}] Removed effects of stage \"{Stage}\".");
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
                Plugin.Log.LogInfo($"[Human:{HumanId}] [{AddictionName}] [Stage]: Worsened to {Stage}.");
        }

        private void MoveToPreviousStage()
        {
            RemoveStageEffects();

            // When we're already in mild, we're cured
            if (Stage == AddictionStage.Mild)
            {
                AddictionManager.Cure(HumanId, AddictionType);
                return;
            }

            Stage--;

            if (Plugin.Instance.Config.DebugMode)
                Plugin.Log.LogInfo($"[Human:{HumanId}] [{AddictionName}] [Stage]: Improved to {Stage}.");
        }
    }
}
