using System;

namespace SOD.Narcotics.AddictionCore.Addictions
{
    public class AlcoholAddiction : Addiction
    {
        public AlcoholAddiction() : base(AddictionType.Alcohol)
        { }

        public override Action<bool> MildStageAction()
        {
            return (apply) =>
            {
                var player = Player.Instance;
                if (apply)
                {
                    AddictionManager.StorePreviousPlayerDataValue("maxHealth_mild", player.maximumHealth);
                    player.SetMaxHealth(Helpers.ApplyPercentageChange(player.maximumHealth, 15, false));
                }
                else
                {
                    var maxHealth = AddictionManager.GetPreviousPlayerDataValue("maxHealth_mild");
                    player.SetMaxHealth(maxHealth);
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
                    AddictionManager.StorePreviousPlayerDataValue("maxHealth_severe", player.maximumHealth);
                    player.SetMaxHealth(Helpers.ApplyPercentageChange(player.maximumHealth, 30, false));
                }
                else
                {
                    var maxHealth = AddictionManager.GetPreviousPlayerDataValue("maxHealth_severe");
                    player.SetMaxHealth(maxHealth);
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
                    AddictionManager.StorePreviousPlayerDataValue("maxHealth_extreme", player.maximumHealth);
                    player.SetMaxHealth(Helpers.ApplyPercentageChange(player.maximumHealth, 50, false));
                }
                else
                {
                    var maxHealth = AddictionManager.GetPreviousPlayerDataValue("maxHealth_extreme");
                    player.SetMaxHealth(maxHealth);
                }
            };
        }
    }
}
