using System;
using System.Collections.Generic;

namespace SOD.Narcotics.Addictions
{
    public static class StageActionFactory
    {
        public static Dictionary<AddictionStage, Action<bool>> GetActions(AddictionType addictionType)
        {
            var actions = new Dictionary<AddictionStage, Action<bool>>();
            foreach (var stage in Enum.GetValues<AddictionStage>())
                actions.Add(stage, GetAction(addictionType, stage));
            return actions;
        }

        public static Action<bool> GetAction(AddictionType addictionType, AddictionStage addictionStage)
        {
            return null; // TODO
        }
    }
}
