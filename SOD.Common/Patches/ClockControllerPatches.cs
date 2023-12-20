using HarmonyLib;
using SOD.Common.Shadows;

namespace SOD.Common.Patches
{
    internal class ClockControllerPatches
    {
        [HarmonyPatch(typeof(ClockController), nameof(ClockController.Update))]
        internal class ClockController_Update
        {
            private static Shadows.Implementations.Time.TimeData? _lastTime;
            private static bool _isInitialized = false;

            [HarmonyPostfix]
            internal static void Postfix()
            {
                if (!SessionData.Instance.play) return;
                if (!_isInitialized)
                {
                    Lib.Time.InitializeTime();
                    _isInitialized = true;
                }

                var currentTime = Lib.Time.CurrentDateTime;
                if (_lastTime == null)
                    _lastTime = currentTime;

                // Check if last time has changed
                if (!_lastTime.Equals(currentTime))
                {
                    Lib.Time.OnTimeChanged(_lastTime.Value, currentTime);
                    _lastTime = currentTime;
                }
            }
        }
    }
}
