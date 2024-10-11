using System;
using System.Collections.Generic;

namespace SOD.Narcotics.AddictionCore.Addictions
{
    public class OpioidAddiction : Addiction
    {
        public static readonly HashSet<string> ExcludedItems = new(StringComparer.OrdinalIgnoreCase)
        {
            "Bandage", "Splint", "HeatPack"
        };

        public OpioidAddiction() : base(AddictionType.Opioid)
        { }

        public override Action<bool> MildStageAction()
        {
            return null;
        }

        public override Action<bool> SevereStageAction()
        {
            return null;
        }

        public override Action<bool> ExtremeStageAction()
        {
            return null;
        }
    }
}
