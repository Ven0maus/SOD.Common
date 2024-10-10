using System;

namespace SOD.Narcotics.AddictionCore.Addictions
{
    internal class CaffeineAddiction : Addiction
    {
        public CaffeineAddiction(int humanId) : base(humanId, AddictionType.Caffeine)
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
