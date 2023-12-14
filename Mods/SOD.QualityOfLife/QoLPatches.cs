using HarmonyLib;
using Rewired;

namespace SOD.QualityOfLife
{
    public class QoLPatches
    {
        [HarmonyPatch(typeof(InputController), nameof(InputController.Update))]
        public class InputController_Update
        {
            [HarmonyPrefix]
            public static bool Prefix(InputController __instance)
            {
                if (!Plugin.Instance.Config.EndConversationPatch && !Plugin.Instance.Config.UnpauseGameOnMainMenuExit) return true;
                if (!__instance.enabled || !ReInput.isReady) return true;

                if (__instance.player != null && __instance.player.GetButtonDown("Menu"))
                {
                    // This allows ending conversations with the menu key
                    if (Plugin.Instance.Config.EndConversationPatch &&
                        SessionData.Instance.startedGame && SessionData.Instance.play && !MainMenuController.Instance.mainMenuActive)
                    {
                        if (Player.Instance.interactingWith != null)
                        {
                            ActionController.Instance.Return(null, null, Player.Instance);
                            return false;
                        }
                    }

                    // This fixes the fact that the game is still paused after you exit the menu with the menu key
                    if (Plugin.Instance.Config.UnpauseGameOnMainMenuExit &&
                        SessionData.Instance.startedGame && !SessionData.Instance.play && MainMenuController.Instance.mainMenuActive &&
                        (CityConstructor.Instance == null || !CityConstructor.Instance.preSimActive))
                    {
                        SessionData.Instance.ResumeGame();
                        MainMenuController.Instance.EnableMainMenu(false, true, true, MainMenuController.Component.mainMenuButtons);
                        return false;
                    }
                }
                return true;
            }
        }
    }
}
