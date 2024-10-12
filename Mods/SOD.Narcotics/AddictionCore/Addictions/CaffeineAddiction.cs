using System;

namespace SOD.Narcotics.AddictionCore.Addictions
{
    internal class CaffeineAddiction : Addiction
    {
        public CaffeineAddiction() : base(AddictionType.Caffeine)
        { }

        public override Action<bool> MildStageAction()
        {
            return (apply) =>
            {
                var player = Player.Instance;
                if (apply)
                {
                    AddictionManager.StorePreviousPlayerDataValue("combatSkill_mild", player.combatSkill);
                    AddictionManager.StorePreviousPlayerDataValue("combatHeft_mild", player.combatHeft);
                    player.SetCombatSkill(Helpers.ApplyPercentageChange(player.combatSkill, 10, false));
                    player.SetCombatHeft(Helpers.ApplyPercentageChange(player.combatSkill, 10, false));
                }
                else
                {
                    var combatSkill = AddictionManager.GetPreviousPlayerDataValue("combatSkill_mild");
                    var combatHeft = AddictionManager.GetPreviousPlayerDataValue("combatHeft_mild");
                    player.SetCombatSkill(combatSkill);
                    player.SetCombatHeft(combatHeft);
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
                    AddictionManager.StorePreviousPlayerDataValue("combatSkill_severe", player.combatSkill);
                    AddictionManager.StorePreviousPlayerDataValue("combatHeft_severe", player.combatHeft);
                    player.SetCombatSkill(Helpers.ApplyPercentageChange(player.combatSkill, 20, false));
                    player.SetCombatHeft(Helpers.ApplyPercentageChange(player.combatSkill, 20, false));
                }
                else
                {
                    var combatSkill = AddictionManager.GetPreviousPlayerDataValue("combatSkill_severe");
                    var combatHeft = AddictionManager.GetPreviousPlayerDataValue("combatHeft_severe");
                    player.SetCombatSkill(combatSkill);
                    player.SetCombatHeft(combatHeft);
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
                    AddictionManager.StorePreviousPlayerDataValue("combatSkill_extreme", player.combatSkill);
                    AddictionManager.StorePreviousPlayerDataValue("combatHeft_extreme", player.combatHeft);
                    player.SetCombatSkill(Helpers.ApplyPercentageChange(player.combatSkill, 30, false));
                    player.SetCombatHeft(Helpers.ApplyPercentageChange(player.combatSkill, 30, false));
                }
                else
                {
                    var combatSkill = AddictionManager.GetPreviousPlayerDataValue("combatSkill_extreme");
                    var combatHeft = AddictionManager.GetPreviousPlayerDataValue("combatHeft_extreme");
                    player.SetCombatSkill(combatSkill);
                    player.SetCombatHeft(combatHeft);
                }
            };
        }
    }
}
