using System;

namespace SOD.Narcotics.AddictionCore.Addictions
{
    public class AlcoholAddiction : Addiction
    {
        public AlcoholAddiction() : base(AddictionType.Alcohol)
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
