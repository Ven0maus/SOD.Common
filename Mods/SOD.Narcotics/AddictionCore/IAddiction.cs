using System;

namespace SOD.Narcotics.AddictionCore
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
        Opioid,
        Sugar,
        Caffeine
    }

    public enum AddictionStage
    {
        Mild,
        Severe,
        Extreme
    }
}
