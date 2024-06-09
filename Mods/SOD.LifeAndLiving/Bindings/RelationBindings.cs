using SOD.Common.BepInEx.Configuration;

namespace SOD.LifeAndLiving.Bindings.RelationBindings
{
    public interface IDialogBindings
    {
        [Binding(10, "The percentage chance of the usual being free.", "LifeAndLiving.RelationsDialog.TheUsualFreeChance")]
        public int TheUsualFreeChance { get; set; }

        [Binding(25, "The percentage chance to get a discount on the usual.", "LifeAndLiving.RelationsDialog.TheUsualDiscountChance")]
        public int TheUsualDiscountChance { get; set; }

        [Binding(35, "The percentage value of the usual's discount.", "LifeAndLiving.RelationsDialog.TheUsualDiscountValuePercentage")]
        public int TheUsualDiscountValue { get; set; }

        [Binding(10, "The percentage chance to get a positive interaction when having certain dialog with an npc.", "LifeAndLiving.RelationsDialog.PositiveInteractionChance")]
        public int PositiveInteractionChance { get; set; }
    }
}
