using HarmonyLib;

namespace SOD.Common.Patches
{
    internal class SessionDataPatches
    {
        private static bool _wasPaused = false;

        [HarmonyPatch(typeof(SessionData), nameof(SessionData.PauseGame))]
        internal static class SessionDataPauseGame
        {
            /// <summary>
            /// OnGamePaused event trigger
            /// </summary>
            [HarmonyPostfix]
            internal static void Postfix()
            {
                if (_wasPaused) return;
                _wasPaused = true;
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
                if (!_wasPaused) return;
                _wasPaused = false;
                Lib.Time.OnPauseModeChanged(false);
            }
        }
    }
}
