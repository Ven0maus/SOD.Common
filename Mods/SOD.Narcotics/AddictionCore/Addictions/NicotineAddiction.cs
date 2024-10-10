using System;

namespace SOD.Narcotics.AddictionCore.Addictions
{
    public class NicotineAddiction : Addiction
    {
        public NicotineAddiction(int humanId) : base(humanId, AddictionType.Nicotine)
        { }

        public override Action<bool> ExtremeStageAction()
        {
            throw new NotImplementedException();
        }

        public override Action<bool> MildStageAction()
        {
            throw new NotImplementedException();
        }

        public override Action<bool> SevereStageAction()
        {
            throw new NotImplementedException();
        }
    }
}
