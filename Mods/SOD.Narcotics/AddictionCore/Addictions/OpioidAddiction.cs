using System;

namespace SOD.Narcotics.AddictionCore.Addictions
{
    public class OpioidAddiction : Addiction
    {
        public OpioidAddiction(int humanId) : base(humanId, AddictionType.Opioid)
        { }

        public override Action<bool> ExtremeStageAction()
        {
            return null;
        }

        public override Action<bool> MildStageAction()
        {
            return null;
        }

        public override Action<bool> SevereStageAction()
        {
            return null;
        }
    }
}
