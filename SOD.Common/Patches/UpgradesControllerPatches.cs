using HarmonyLib;
using SOD.Common.Helpers;

namespace SOD.Common.Patches
{
    internal class UpgradesControllerPatches
    {
        [HarmonyPatch(typeof(UpgradesController), nameof(UpgradesController.InstallSyncDisk))]
        internal class InstallSyncDiskHook
        {
            [HarmonyPrefix]
            internal static void Prefix(UpgradesController.Upgrades application, int option)
            {
                if (application == null || application.preset == null) return;
                Lib.SyncDisks.RaiseSyncDiskEvent(SyncDisks.SyncDiskEvent.OnInstall, false, application, option);
            }

            [HarmonyPostfix]
            public static void Postfix(UpgradesController.Upgrades application, int option)
            {
                if (application == null || application.preset == null) return;
                Lib.SyncDisks.RaiseSyncDiskEvent(SyncDisks.SyncDiskEvent.OnInstall, true, application, option);
            }
        }

        [HarmonyPatch(typeof(UpgradesController), nameof(UpgradesController.UpgradeSyncDisk))]
        public class UpgradeSyncDiskHook
        {
            [HarmonyPrefix]
            public static void Prefix(UpgradesController.Upgrades upgradeThis)
            {
                if (upgradeThis == null || upgradeThis.preset == null) return;
                Lib.SyncDisks.RaiseSyncDiskEvent(SyncDisks.SyncDiskEvent.OnUpgrade, false, upgradeThis);
            }

            [HarmonyPostfix]
            public static void Postfix(UpgradesController.Upgrades upgradeThis)
            {
                if (upgradeThis == null || upgradeThis.preset == null) return;
                Lib.SyncDisks.RaiseSyncDiskEvent(SyncDisks.SyncDiskEvent.OnUpgrade, true, upgradeThis);
            }
        }

        [HarmonyPatch(typeof(UpgradesController), nameof(UpgradesController.UninstallSyncDisk))]
        public class UninstallSyncDiskHook
        {
            [HarmonyPrefix]
            public static void Prefix(UpgradesController.Upgrades removal)
            {
                if (removal == null || removal.preset == null) return;
                Lib.SyncDisks.RaiseSyncDiskEvent(SyncDisks.SyncDiskEvent.OnUninstall, false, removal);
            }

            [HarmonyPostfix]
            public static void Postfix(UpgradesController.Upgrades removal)
            {
                if (removal == null || removal.preset == null) return;
                Lib.SyncDisks.RaiseSyncDiskEvent(SyncDisks.SyncDiskEvent.OnUninstall, true, removal);
            }
        }
    }
}
