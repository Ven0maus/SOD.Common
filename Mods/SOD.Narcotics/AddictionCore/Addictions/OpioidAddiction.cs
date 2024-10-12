using System;
using System.Collections.Generic;

namespace SOD.Narcotics.AddictionCore.Addictions
{
    public class OpioidAddiction : Addiction
    {
        public static readonly HashSet<string> ExcludedItems = new(StringComparer.OrdinalIgnoreCase)
        {
            "Bandage", "Splint", "HeatPack"
        };

        public OpioidAddiction() : base(AddictionType.Opioid)
        { }

        public override Action<bool> MildStageAction()
        {
            return (apply) =>
            {
                var player = Player.Instance;
                if (apply)
                {
                    AddictionManager.StorePreviousPlayerDataValue("recoveryRate_mild", player.recoveryRate);
                    player.SetRecoveryRate(Helpers.ApplyPercentageChange(player.recoveryRate, 20, false));
                }
                else
                {
                    var recoveryRate = AddictionManager.GetPreviousPlayerDataValue("recoveryRate_mild");
                    player.SetRecoveryRate(recoveryRate);
                }
            };
        }

        public override Action<bool> SevereStageAction()
        {
            return (apply) =>
            {
                var player = Player.Instance;
                if (apply)
                {
                    AddictionManager.StorePreviousPlayerDataValue("recoveryRate_severe", player.recoveryRate);
                    player.SetRecoveryRate(Helpers.ApplyPercentageChange(player.recoveryRate, 40, false));
                }
                else
                {
                    var recoveryRate = AddictionManager.GetPreviousPlayerDataValue("recoveryRate_severe");
                    player.SetRecoveryRate(recoveryRate);
                }
            };
        }

        public override Action<bool> ExtremeStageAction()
        {
            return (apply) =>
            {
                var player = Player.Instance;
                if (apply)
                {
                    AddictionManager.StorePreviousPlayerDataValue("recoveryRate_extreme", player.recoveryRate);
                    player.SetRecoveryRate(Helpers.ApplyPercentageChange(player.recoveryRate, 60, false));
                }
                else
                {
                    var recoveryRate = AddictionManager.GetPreviousPlayerDataValue("recoveryRate_extreme");
                    player.SetRecoveryRate(recoveryRate);
                }
            };
        }
    }
}
