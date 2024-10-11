using System;

namespace SOD.Narcotics.AddictionCore.Addictions
{
    internal class CaffeineAddiction : Addiction
    {
        public CaffeineAddiction() : base(AddictionType.Caffeine)
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
