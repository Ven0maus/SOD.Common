using System;

namespace SOD.Narcotics.AddictionCore.Addictions
{
    public class NicotineAddiction : Addiction
    {
        public NicotineAddiction(int humanId) : base(humanId, AddictionType.Nicotine)
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
