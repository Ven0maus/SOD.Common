using SOD.Common.Helpers.SyncDiskObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SOD.Common.Helpers
{
    public sealed class SyncDisks
    {
        internal SyncDisks() { }

        private static int? _currentSyncDiskEffectId = null;
        /// <summary>
        /// Adds a new number at the end of the available effects,
        /// <br>This should automatically increase if the game adds new effects.</br>
        /// <br>All mods will then always get the right effect ids.</br>
        /// </summary>
        /// <returns></returns>
        internal static int GetNewSyncDiskEffectId()
        {
            if (_currentSyncDiskEffectId == null)
            {
                int[] values = (int[])Enum.GetValues(typeof(SyncDiskPreset.Effect));
                _currentSyncDiskEffectId = values.DefaultIfEmpty().Max();
            }
            _currentSyncDiskEffectId++;
            return _currentSyncDiskEffectId.Value;
        }

        /// <summary>
        /// All the effects in here have been registered by mods and are available in the game.
        /// </summary>
        internal static HashSet<Effect> RegisteredEffects = new();

        /// <summary>
        /// Returns a set of all the known effects that have so far been registered by mods using SOD.Common Sync Disks helper functionality.
        /// </summary>
        public static IReadOnlySet<Effect> KnownModEffects => RegisteredEffects;

        /// <summary>
        /// Raised before a new sync disk is installed on the player.
        /// </summary>
        public event EventHandler<SyncDiskInstallUpgradeArgs> OnBeforeSyncDiskInstalled;
        /// <summary>
        /// Raised after a new sync disk is installed on the player.
        /// </summary>
        public event EventHandler<SyncDiskInstallUpgradeArgs> OnAfterSyncDiskInstalled;
        /// <summary>
        /// Raised before an existing sync disk is upgraded on the player.
        /// </summary>
        public event EventHandler<SyncDiskInstallUpgradeArgs> OnBeforeSyncDiskUpgraded;
        /// <summary>
        /// Raised after an existing sync disk is upgraded on the player.
        /// </summary>
        public event EventHandler<SyncDiskInstallUpgradeArgs> OnAfterSyncDiskUpgraded;
        /// <summary>
        /// Raised before an existing sync disk is uninstalled on the player.
        /// </summary>
        public event EventHandler<SyncDiskArgs> OnBeforeSyncDiskUninstalled;
        /// <summary>
        /// Raised after an existing sync disk is uninstalled on the player.
        /// </summary>
        public event EventHandler<SyncDiskArgs> OnAfterSyncDiskUninstalled;

        /// <summary>
        /// Creates a builder object that can help you build a custom sync disk.
        /// <br>When finished setting up your properties on the builder, call builder.Create();</br>
        /// <br>This will return a SyncDisk object which you can call .Register() on the register it into the game.</br>
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public SyncDiskBuilder Builder(string syncDiskName)
        {
            return new SyncDiskBuilder(syncDiskName);
        }

        internal void RaiseSyncDiskEvent(SyncDiskEvent syncDiskEvent, bool after, UpgradesController.Upgrades upgrade, int? option = null)
        {
            switch (syncDiskEvent)
            {
                case SyncDiskEvent.OnInstall:
                    if (after)
                        OnAfterSyncDiskInstalled?.Invoke(this, new SyncDiskInstallUpgradeArgs(upgrade, option));
                    else
                        OnBeforeSyncDiskInstalled?.Invoke(this, new SyncDiskInstallUpgradeArgs(upgrade, option));
                    break;
                case SyncDiskEvent.OnUninstall:
                    if (after)
                        OnAfterSyncDiskUninstalled?.Invoke(this, new SyncDiskInstallUpgradeArgs(upgrade));
                    else
                        OnBeforeSyncDiskUninstalled?.Invoke(this, new SyncDiskInstallUpgradeArgs(upgrade));
                    break;
                case SyncDiskEvent.OnUpgrade:
                    if (after)
                        OnAfterSyncDiskUpgraded?.Invoke(this, new SyncDiskInstallUpgradeArgs(upgrade));
                    else
                        OnBeforeSyncDiskUpgraded?.Invoke(this, new SyncDiskInstallUpgradeArgs(upgrade));
                    break;
                default:
                    throw new NotSupportedException($"Invalid event: {syncDiskEvent}");
            }
        }

        internal enum SyncDiskEvent
        {
            OnInstall,
            OnUninstall,
            OnUpgrade
        }

        public readonly struct Effect : IEquatable<Effect>
        {
            public readonly int Id;
            public readonly string Name;

            public Effect(int id, string name)
            {
                Id = id;
                Name = name;
            }

            public bool Equals(Effect other)
            {
                return Id == other.Id;
            }

            public override bool Equals(object obj)
            {
                return obj is Effect effect && Equals(effect);
            }

            public static bool operator ==(Effect left, Effect right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(Effect left, Effect right)
            {
                return !(left == right);
            }

            public override int GetHashCode()
            {
                return Id.GetHashCode();
            }
        }
    }
}
