using System;

namespace SOD.Narcotics.AddictionCore.Addictions
{
    public class AlcoholAddiction : Addiction
    {
        public AlcoholAddiction() : base(AddictionType.Alcohol)
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
