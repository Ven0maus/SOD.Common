using HarmonyLib;
using SOD.Common.Helpers;

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
                Lib.Time.IsGamePaused = true;
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
                Lib.Time.IsGamePaused = false;
                Lib.Time.OnPauseModeChanged(false);
            }
        }

        [HarmonyPatch(typeof(SessionData), nameof(SessionData.Update))]
        internal static class SessionData_Update
        {
            internal static Time.TimeData? LastTime;

            [HarmonyPostfix]
            internal static void Postfix()
            {
                if (!SessionData.Instance.play) return;

                // Init time
                if (!Lib.Time.IsInitialized)
                    Lib.Time.InitializeTime();

                var currentTime = Lib.Time.CurrentDateTime;
                if (LastTime == null)
                    LastTime = currentTime;

                // Check if last time has changed
                if (!LastTime.Equals(currentTime))
                {
                    Lib.Time.OnTimeChanged(LastTime.Value, currentTime);
                    LastTime = currentTime;
                }
            }
        }

        [HarmonyPatch(typeof(SessionData), nameof(SessionData.Start))]
        internal static class SessionData_Start
        {
            [HarmonyPostfix]
            internal static void Postfix()
            {
                Lib.Time.InitializeTime();
            }
        }
    }
}
