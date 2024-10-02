using System;

namespace SOD.Narcotics.AddictionCore.Addictions
{
    public class OpioidAddiction : Addiction
    {
        public OpioidAddiction() : base(AddictionType.Opioid)
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
