using SOD.Common.BepInEx.Configuration;

namespace SOD.QoL
{
    public interface IPluginBindings : IConversationBindings, IMainMenuBindings, IMapBindings, IGameplayPatchBindings, IJobExpirationBindings
    { }

    public interface IGameplayPatchBindings
    {
        [Binding(true, "Fixes the player never getting tired.", "QoL.Gameplay.FixTiredness")]
        bool FixTiredness { get; set; }

        [Binding(12, "The percentage that is taken of alertness and added to energy restore for caffeine items. (12 seems balanced)", "QoL.Gameplay.PercentageEnergyRestore")]
        int PercentageEnergyRestore { get; set; }

        [Binding(true, "Adds a link to the address from the wallet.", "QoL.Gameplay.AddWalletLinkToAddress")]
        bool AddWalletLinkToAddress { get; set; }

        [Binding(true, "Adds a link to the address from the employment contract.", "QoL.Gameplay.AddEmploymentContractLinkToAddress")]
        bool AddEmploymentContractLinkToAddress { get; set; }
    }

    public interface IJobExpirationBindings
    {
        [Binding(true, "Side jobs and LostAndFound jobs will automatically expire after some in-game hours to prevent stale evidence. (accepted side jobs are excluded)", "QoL.Gameplay.AutoExpireJobs")]
        bool AutoExpireJobs { get; set; }

        [Binding(true, "Should the expire time be randomized for more immersion in the game world? (If false, it will take ExpireTimeMax)", "QoL.Gameplay.RandomizeExpireTime")]
        bool RandomizeExpireTime { get; set; }

        [Binding(24, "The minimum time for expiration to occur.", "QoL.Gameplay.ExpireTimeMin")]
        int ExpireTimeMin { get; set; }

        [Binding(48, "The maximum time for expiration to occur.", "QoL.Gameplay.ExpireTimeMax")]
        int ExpireTimeMax { get; set; }
    }

    public interface IConversationBindings
    {
        [Binding(true, "Allow ending conversations with menu key", "QoL.Conversations.CanEndWithMenuKey")]
        bool EndConversationPatch { get; set; }

        [Binding(true, "Allow skipping a conversation option by clicking again.", "QoL.Conversations.CanSkipConversationOption")]
        bool EnableSkipConversationPatch { get; set; }
    }

    public interface IMainMenuBindings
    {
        [Binding(true, "Resume the game from paused state when exiting the mainmenu with menu key", "QoL.MainMenu.UnpauseGameWithMenuKey")]
        bool UnpauseGameOnMainMenuExit { get; set; }

        [Binding(true, "Skips the press any key screen at the start of the game if no joysticks are connected.", "QoL.MainMenu.SkipPressAnyKeyScreenIfNotUsingJoysticks")]
        bool SkipPressAnyKeyScreenIfNotUsingJoysticks { get; set; }
    }

    public interface IMapBindings
    {
        [Binding(true, "Fixes the center on player after zooming and moving the camera.", "QoL.Map.FixCenterOnPlayer")]
        bool FixCenterOnPlayer { get; set; }

        [Binding(true, "Zooms the minimap out by default?", "QoL.Map.ZoomOutOnStart")]
        bool ZoomOutOnStart { get; set; }

        [Binding(true, "Enlarge player marker?", "QoL.Map.EnlargePlayerMarker")]
        bool EnlargePlayerMarker { get; set; }

        [Binding("(2.5, 2.5)", "Player marker size. Game default: (1,1)", "QoL.Map.PlayerMarkerSize")]
        string PlayerMarkerSize { get; set; }

        [Binding(true, "Change player marker color?", "QoL.Map.ChangePlayerMarkerColor")]
        bool ChangePlayerMarkerColor { get; set; }

        [Binding("#008000", "The hex color code for the player marker, default: Green", "QoL.Map.PlayerMarkerColor")]
        string PlayerMarkerColor { get; set; }
    }
}
