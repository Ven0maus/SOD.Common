using System;
using System.Collections.Generic;
using System.Linq;

namespace SOD.Narcotics.AddictionCore
{
    public abstract class Addiction : IAddiction
    {
        public string AddictionName => $"{AddictionType} addiction";
        public AddictionType AddictionType { get; }
        public AddictionStage Stage { get; set; }
        public HashSet<AddictionStage> AppliedStageEffects { get; set; } = new();

        public Addiction(AddictionType addictionType)
        {
            AddictionType = addictionType;
            Stage = AddictionStage.Mild;
        }

        public void Cure()
        {
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

        public void MoveToNextStage()
        {
            if (Stage == AddictionStage.Extreme) return;
            Stage++;
            ApplyStageEffects();

            if (Plugin.Instance.Config.DebugMode)
                Plugin.Log.LogInfo($"[{AddictionName}] [Stage]: Worsened to {Stage}.");
        }

        public void MoveToPreviousStage()
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
