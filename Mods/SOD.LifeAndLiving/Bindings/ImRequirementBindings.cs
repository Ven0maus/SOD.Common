using SOD.Common.BepInEx.Configuration;

namespace SOD.LifeAndLiving.Bindings.ImRequirementBindings
{
    public interface ImRequirementBindings
    {
        [Binding(true, "Should the player need a screwdriver in their inventory to open vents?", "LifeAndLiving.ImmersiveRequirements.RequireScrewdriverForVents")]
        bool RequireScrewdriverForVents { get; set; }
    }
}
