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
                    // Apply mild positive effects
                    player.SetMaxSpeed(player.movementWalkSpeed + 0.5f, player.movementRunSpeed + 0.5f);
                }
                else
                {
                    // Remove effects when addiction is not active or relieved
                    player.SetMaxSpeed(player.movementWalkSpeed - 0.5f, player.movementRunSpeed - 0.5f);
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
                    // Apply mild positive effects
                    player.SetMaxSpeed(player.movementWalkSpeed - 0.75f, player.movementRunSpeed - 0.75f);
                }
                else
                {
                    // Remove effects when addiction is not active or relieved
                    player.SetMaxSpeed(player.movementWalkSpeed + 0.75f, player.movementRunSpeed + 0.75f);
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
                    // Apply mild positive effects
                    player.SetMaxSpeed(player.movementWalkSpeed - 0.75f, player.movementRunSpeed - 0.75f);
                }
                else
                {
                    // Remove effects when addiction is not active or relieved
                    player.SetMaxSpeed(player.movementWalkSpeed + 0.75f, player.movementRunSpeed + 0.75f);
                }
            };
        }
    }
}
