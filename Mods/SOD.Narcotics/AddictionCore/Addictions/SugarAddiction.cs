using System;
using System.Collections.Generic;

namespace SOD.Narcotics.AddictionCore.Addictions
{
    internal class SugarAddiction : Addiction
    {
        public static readonly Dictionary<string, float> Sugars = new(StringComparer.OrdinalIgnoreCase)
        {
            { "Donut", 1f },
            { "Eclair", 1f },
            { "ChocolateBar", 1.25f },
            { "StarchCandy", 1.25f },
            { "StarchKola", 1.5f },
        };

        public SugarAddiction() : base(AddictionType.Sugar)
        { }

        public override Action<bool> MildStageAction()
        {
            return (apply) =>
            {
                var player = Player.Instance;
                if (apply)
                {
                    AddictionManager.StorePreviousPlayerDataValue("walkSpeed_mild", player.movementWalkSpeed);
                    AddictionManager.StorePreviousPlayerDataValue("runSpeed_mild", player.movementRunSpeed);
                    player.SetMaxSpeed(Helpers.ApplyPercentageChange(player.movementWalkSpeed, 0, false), 
                        Helpers.ApplyPercentageChange(player.movementRunSpeed, 5, false));
                }
                else
                {
                    var walkSpeed = AddictionManager.GetPreviousPlayerDataValue("walkSpeed_mild");
                    var runSpeed = AddictionManager.GetPreviousPlayerDataValue("runSpeed_mild");
                    player.SetMaxSpeed(walkSpeed, runSpeed);
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
                    AddictionManager.StorePreviousPlayerDataValue("walkSpeed_severe", player.movementWalkSpeed);
                    AddictionManager.StorePreviousPlayerDataValue("runSpeed_severe", player.movementRunSpeed);
                    player.SetMaxSpeed(Helpers.ApplyPercentageChange(player.movementWalkSpeed, 5, false),
                        Helpers.ApplyPercentageChange(player.movementRunSpeed, 15, false));
                }
                else
                {
                    var walkSpeed = AddictionManager.GetPreviousPlayerDataValue("walkSpeed_severe");
                    var runSpeed = AddictionManager.GetPreviousPlayerDataValue("runSpeed_severe");
                    player.SetMaxSpeed(walkSpeed, runSpeed);
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
                    AddictionManager.StorePreviousPlayerDataValue("walkSpeed_extreme", player.movementWalkSpeed);
                    AddictionManager.StorePreviousPlayerDataValue("runSpeed_extreme", player.movementRunSpeed);
                    player.SetMaxSpeed(Helpers.ApplyPercentageChange(player.movementWalkSpeed, 10, false),
                        Helpers.ApplyPercentageChange(player.movementRunSpeed, 30, false));
                }
                else
                {
                    var walkSpeed = AddictionManager.GetPreviousPlayerDataValue("walkSpeed_extreme");
                    var runSpeed = AddictionManager.GetPreviousPlayerDataValue("runSpeed_extreme");
                    player.SetMaxSpeed(walkSpeed, runSpeed);
                }
            };
        }
    }
}
