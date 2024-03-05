using SOD.Common.BepInEx.Configuration;

namespace SOD.QoL
{
    public interface IPluginBindings : IConversationBindings, IMainMenuBindings, IMapBindings
    { }

    public interface IConversationBindings
    {
        [Binding(true, "Allow ending conversations with menu key", "QoL.Conversations.CanEndWithMenuKey")]
        bool EndConversationPatch { get; set; }
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
