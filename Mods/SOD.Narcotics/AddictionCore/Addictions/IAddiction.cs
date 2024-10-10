using System;

namespace SOD.Narcotics.AddictionCore.Addictions
{
    internal interface IAddiction
    {
        Action<bool> MildStageAction();
        Action<bool> SevereStageAction();
        Action<bool> ExtremeStageAction();
    }

    public enum AddictionType
    {
        Alcohol,
        Nicotine,
        Opioid
    }

    public enum AddictionStage
    {
        Mild,
        Severe,
        Extreme
    }
}
