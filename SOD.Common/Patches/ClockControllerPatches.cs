using HarmonyLib;
using SOD.Common.Shadows;

namespace SOD.StockMarket.Patches
{
    internal class ClockControllerPatches
    {
        [HarmonyPatch(typeof(ClockController), nameof(ClockController.Update))]
        internal class ClockController_Update
        {
            private static Common.Shadows.Implementations.Time.TimeData? _lastTime;

            [HarmonyPostfix]
            internal static void Postfix()
            {
                if (!Lib.Time.IsInitialized) return;

                var currentTime = Lib.Time.CurrentDateTime;
                if (_lastTime == null || !_lastTime.Equals(currentTime))
                {
                    Lib.Time.OnTimeChanged(_lastTime.Value, currentTime);
                    _lastTime = currentTime;
                }
            }
        }
    }
}
