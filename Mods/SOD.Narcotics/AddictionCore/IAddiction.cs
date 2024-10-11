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
        Alcohol = 0,
        Nicotine = 1,
        Opioid = 2,
        Sugar = 3,
        Caffeine = 4
    }

    public enum AddictionStage
    {
        Mild = 0,
        Severe = 1,
        Extreme = 2
    }
}
