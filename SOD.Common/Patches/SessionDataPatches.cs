using HarmonyLib;

namespace SOD.Common.Patches
{
    internal class SessionDataPatches
    {
        private static bool _wasSaved = false;

        [HarmonyPatch(typeof(SessionData), nameof(SessionData.PauseGame))]
        internal static class SessionDataPauseGame
        {
            /// <summary>
            /// OnGamePaused event trigger
            /// </summary>
            [HarmonyPostfix]
            internal static void Postfix()
            {
                if (_wasSaved) return;
                _wasSaved = true;
                Lib.Time.OnPauseModeChanged(true);
            }
        }

        [HarmonyPatch(typeof(SessionData), nameof(SessionData.ResumeGame))]
        internal static class SessionDataResumeGame
        {
            /// <summary>
            /// OnGameResumed event trigger
            /// </summary>
            [HarmonyPostfix]
            internal static void Postfix()
            {
                if (!_wasSaved) return;
                _wasSaved = false;
                Lib.Time.OnPauseModeChanged(false);
            }
        }
    }
}
