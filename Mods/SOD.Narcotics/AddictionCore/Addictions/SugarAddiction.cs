using System;

namespace SOD.Narcotics.AddictionCore.Addictions
{
    internal class SugarAddiction : Addiction
    {
        public SugarAddiction(int humanId) : base(humanId, AddictionType.Sugar)
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
