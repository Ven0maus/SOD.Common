using System;
using System.Collections.Generic;

namespace SOD.Narcotics.AddictionCore
{
    public class AddictionManager
    {
        private readonly Dictionary<AddictionType, Addiction> _addictions = new();
        private readonly List<AddictionType> _toBeCured = new();

        /// <summary>
        /// Assigns a new addiction.
        /// </summary>
        /// <param name="addictionType"></param>
        /// <param name="stage"></param>
        public void Assign(AddictionType addictionType, AddictionStage stage = AddictionStage.Mild)
        {
            if (!_addictions.TryGetValue(addictionType, out _))
            {
                // Get a new addiction class
                Addiction addiction = AddictionFactory.Get(addictionType);
                addiction.IsActive = true;

                // Add to collection
                _addictions.Add(addictionType, addiction);

                // Update for each stage
                for (int i = 0; i <= (int)stage; i++)
                {
                    addiction.Stage = (AddictionStage)i;
                    addiction.Update();
                }
            }
        }

        /// <summary>
        /// Cures the given addiction.
        /// </summary>
        /// <param name="addictionType"></param>
        public void Cure(AddictionType addictionType)
        {
            if (_addictions.TryGetValue(addictionType, out var addiction))
            {
                _addictions.Remove(addictionType);

                // Remove effects and disable addiction
                addiction.Cure();
            }
        }

        /// <summary>
        /// Set's the current state of the addiction. (recovering or not)
        /// </summary>
        /// <param name="addictionType"></param>
        /// <param name="recovering"></param>
        public void SetAddictionState(AddictionType addictionType, bool recovering)
        {
            if (_addictions.TryGetValue(addictionType, out var addiction))
                addiction.Recovering = recovering;
        }

        public void Update(float deltaTime)
        {
            foreach (var addiction in _addictions.Values)
            {
                if (!addiction.IsActive)
                {
                    _toBeCured.Add(addiction.AddictionType);
                    continue;
                }

                // TODO: Check how this progression works in game by hooking it up to an update method
                // Define if progression is going forward or backwards based on recovering or not
                var progress = deltaTime / (float)addiction.RelapseTime.TotalSeconds;
                if (addiction.Recovering)
                    addiction.StageProgress -= progress;
                else
                    addiction.StageProgress += progress;
               
                // Once progression reaches 0 or 100 we adjust the stage
                if (addiction.StageProgress <= 0f || addiction.StageProgress >= 100f)
                {
                    if (addiction.StageProgress <= 0f)
                    {
                        // Cure when we are at stage 0 and trying to reduce further.
                        if (addiction.Stage == 0)
                        {
                            _toBeCured.Add(addiction.AddictionType);
                            continue;
                        }
                        addiction.Stage -= 1;
                    }
                    else if (addiction.StageProgress >= 100f)
                    {
                        addiction.Stage += 1;
                    }

                    // Adjust stage properly with clamped value and reset progress
                    var previousStage = addiction.Stage;
                    addiction.Stage = (AddictionStage)Math.Clamp((int)addiction.Stage, 0, 2);
                    addiction.StageProgress = 0f;

                    // Update the addiction if the stage changed
                    if (previousStage != addiction.Stage)
                        addiction.Update();
                }
            }

            // Cure addictions if needed
            foreach (var addiction in _toBeCured)
                Cure(addiction);
            if (_toBeCured.Count > 0)
                _toBeCured.Clear();
        }
    }
}
