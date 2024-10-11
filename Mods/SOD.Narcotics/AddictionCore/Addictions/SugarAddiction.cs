using System;

namespace SOD.Narcotics.AddictionCore.Addictions
{
    internal class SugarAddiction : Addiction
    {
        public SugarAddiction() : base(AddictionType.Sugar)
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
