using SOD.Common;
using SOD.Common.Helpers;
using SOD.Narcotics.AddictionCore.Addictions;
using System;
using System.Collections.Generic;

namespace SOD.Narcotics.AddictionCore
{
    public abstract class Addiction : IAddiction
    {
        public string AddictionName => $"{AddictionType} addiction";
        public AddictionType AddictionType { get; private set; }
        public float StageProgress { get; set; }
        public bool Recovering
        {
            get
            {
                return _timeSinceLastWorsening.AddHours(6) <= Lib.Time.CurrentDateTime;
            }
        }
        public AddictionStage Stage { get; private set; }
        public int HumanId { get; }

        private Time.TimeData _timeSinceLastWorsening;
        private readonly HashSet<AddictionStage> _appliedStageEffects = new();

        public Addiction(int humanId, AddictionType addictionType)
        {
            HumanId = humanId;
            AddictionType = addictionType;
            Stage = AddictionStage.Mild;
            _timeSinceLastWorsening = Lib.Time.CurrentDateTime;

            if (Plugin.Instance.Config.DebugMode)
                Plugin.Log.LogInfo($"[Human: {HumanId}] has become addicted to \"{AddictionType.ToString().ToLower()}\".");

            ApplyStageEffects();
        }

        /// <summary>
        /// Worsens the addictions
        /// </summary>
        /// <param name="progressAmount"></param>
        public void AdjustProgress(float progressAmount)
        {
            if (progressAmount > 0)
                _timeSinceLastWorsening = Lib.Time.CurrentDateTime;

            StageProgress += progressAmount;

            if (StageProgress >= 1f && Stage < AddictionStage.Extreme)
            {
                StageProgress = 0f; // Reset progress for the next stage
                MoveToNextStage();
            }
            else if (StageProgress <= 0f)
            {
                StageProgress = 0f; // Reset progress for the previous stage
                MoveToPreviousStage();
            }

            if (Plugin.Instance.Config.DebugMode)
                Plugin.Log.LogInfo($"[Human:{HumanId}] [{AddictionName}] [Progress]: {StageProgress * 100}% towards {(progressAmount > 0 ? "worsening" : "recovery")}.");
        }

        public void Cure()
        {
            StageProgress = 0f;

            // Remove all effects
            for (int i=(int)Stage; i >= 0; i--)
            {
                Stage = (AddictionStage)i;
                RemoveStageEffects();
            }

            if (Plugin.Instance.Config.DebugMode)
                Plugin.Log.LogInfo($"[Human:{HumanId}] has now been cured of his \"{AddictionName}\".");
        }

        public abstract Action<bool> MildStageAction();
        public abstract Action<bool> SevereStageAction();
        public abstract Action<bool> ExtremeStageAction();

        private void ApplyStageEffects()
        {
            if (_appliedStageEffects.Contains(Stage)) return;
            GetStageAction()?.Invoke(true);
            _appliedStageEffects.Add(Stage);

            if (Plugin.Instance.Config.DebugMode)
                Plugin.Log.LogInfo($"[Human:{HumanId}] [{AddictionName}] Applied effects of stage \"{Stage}\".");
        }

        private void RemoveStageEffects()
        {
            if (!_appliedStageEffects.Contains(Stage)) return;
            GetStageAction()?.Invoke(false);
            _appliedStageEffects.Remove(Stage);

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
