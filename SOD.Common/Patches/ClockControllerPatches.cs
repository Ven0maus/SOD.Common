using HarmonyLib;
using SOD.Common.Helpers;

namespace SOD.Common.Patches
{
    internal class ClockControllerPatches
    {
        [HarmonyPatch(typeof(ClockController), nameof(ClockController.Update))]
        internal class ClockController_Update
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
    }
}
