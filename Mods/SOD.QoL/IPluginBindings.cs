using SOD.Common.BepInEx.Configuration;

namespace SOD.QoL
{
    public interface IPluginBindings
    {
        [Binding(true, "Allow ending conversations with menu key", "QoL.Conversations.CanEndWithMenuKey")]
        bool EndConversationPatch { get; set; }

        [Binding(true, "Resume the game from paused state when exiting the mainmenu with menu key", "QoL.MainMenu.UnpauseGameWithMenuKey")]
        bool UnpauseGameOnMainMenuExit { get; set; }
    }
}
