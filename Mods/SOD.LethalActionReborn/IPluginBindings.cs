using SOD.Common.BepInEx.Configuration;

namespace SOD.LethalActionReborn
{
    public interface IPluginBindings
    {
        [Binding(1, "How many hits on the knocked out citizen are required to kill the citizen by the player?", "Citizens.HitsRequiredForKillAfterKo")]
        int HitsRequiredForKillAfterKo { get; set; }

        [Binding("", "Which weapon types won't kill citizens when attacked with by the player? (Comma-seperated) Available types are: fists,rifle,blade,bluntObject,handgun,shotgun", "Weapons.WeaponTypesExcludedFromKilling")]
        string WeaponTypesExcludedFromKilling { get; set; }
    }
}
