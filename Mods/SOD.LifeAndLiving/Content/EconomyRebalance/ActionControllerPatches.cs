using HarmonyLib;

namespace SOD.LifeAndLiving.Patches.EconomyRebalancePatches
{
    internal class ActionControllerPatches
    {
        [HarmonyPatch(typeof(ActionController), nameof(ActionController.TakeLockpickKit))]
        internal class ActionController_TakeLockpickKit
        {
            // Change the amount of lockpicks the kit contains
            private static int _previousLockPicks;

            [HarmonyPrefix]
            internal static void Prefix()
            {
                _previousLockPicks = GameplayController.Instance.lockPicks;
            }

            [HarmonyPostfix]
            internal static void Postfix()
            {
                if (Plugin.Instance.Config.DisableEconomyRebalance) return;
                var currentLockpicks = GameplayController.Instance.lockPicks;
                var differenceLockpicks = currentLockpicks - _previousLockPicks;

                // Game adds by the difference, so we reduce by that amount + add the config amount we want
                GameplayController.Instance.AddLockpicks(-differenceLockpicks + Plugin.Instance.Config.LockPickKitAmount, false);
            }
        }
    }
}
