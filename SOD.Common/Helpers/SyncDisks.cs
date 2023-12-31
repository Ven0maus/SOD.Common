﻿using SOD.Common.Helpers.SyncDiskObjects;
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

        private static int? _currentSyncDiskOptionId = null;
        /// <summary>
        /// Adds a new number at the end of the available upgrade options,
        /// <br>This should automatically increase if the game adds new options.</br>
        /// <br>All mods will then always get the right option ids.</br>
        /// </summary>
        /// <returns></returns>
        internal static int GetNewSyncDiskOptionId()
        {
            if (_currentSyncDiskOptionId == null)
            {
                int[] values = (int[])Enum.GetValues(typeof(SyncDiskPreset.UpgradeEffect));
                _currentSyncDiskOptionId = values.DefaultIfEmpty().Max();
            }
            _currentSyncDiskOptionId++;
            return _currentSyncDiskOptionId.Value;
        }

        /// <summary>
        /// All the registered sync disks in the game by mods.
        /// </summary>
        internal List<SyncDisk> RegisteredSyncDisks = new();

        /// <summary>
        /// Returns a list of all the known sync disks that have so far been registered by mods using SOD.Common Sync Disks helper functionality.
        /// </summary>
        public IReadOnlyList<SyncDisk> KnownModSyncDisks => RegisteredSyncDisks;

        /// <summary>
        /// Raised before a new sync disk is installed on the player.
        /// </summary>
        public event EventHandler<SyncDiskArgs> OnBeforeSyncDiskInstalled;
        /// <summary>
        /// Raised after a new sync disk is installed on the player.
        /// </summary>
        public event EventHandler<SyncDiskArgs> OnAfterSyncDiskInstalled;
        /// <summary>
        /// Raised before an existing sync disk is upgraded on the player.
        /// </summary>
        public event EventHandler<SyncDiskArgs> OnBeforeSyncDiskUpgraded;
        /// <summary>
        /// Raised after an existing sync disk is upgraded on the player.
        /// </summary>
        public event EventHandler<SyncDiskArgs> OnAfterSyncDiskUpgraded;
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

        internal void RaiseSyncDiskEvent(SyncDiskEvent syncDiskEvent, bool after, UpgradesController.Upgrades upgrade)
        {
            switch (syncDiskEvent)
            {
                case SyncDiskEvent.OnInstall:
                    if (after)
                        OnAfterSyncDiskInstalled?.Invoke(this, new SyncDiskArgs(upgrade, true, false));
                    else
                        OnBeforeSyncDiskInstalled?.Invoke(this, new SyncDiskArgs(upgrade, true, false));
                    break;
                case SyncDiskEvent.OnUninstall:
                    if (after)
                        OnAfterSyncDiskUninstalled?.Invoke(this, new SyncDiskArgs(upgrade, true, false));
                    else
                        OnBeforeSyncDiskUninstalled?.Invoke(this, new SyncDiskArgs(upgrade, true, false));
                    break;
                case SyncDiskEvent.OnUpgrade:
                    if (after)
                        OnAfterSyncDiskUpgraded?.Invoke(this, new SyncDiskArgs(upgrade));
                    else
                        OnBeforeSyncDiskUpgraded?.Invoke(this, new SyncDiskArgs(upgrade));
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

        public readonly struct UpgradeOption : IEquatable<UpgradeOption>
        {
            public readonly int Id1, Id2, Id3;
            public readonly string Name1, Name2, Name3;

            internal readonly bool HasOptions => !string.IsNullOrWhiteSpace(Name1) || !string.IsNullOrWhiteSpace(Name2) || !string.IsNullOrWhiteSpace(Name3);
            internal readonly SyncDiskBuilder.Options Options;

            public UpgradeOption(SyncDiskBuilder.Options options, bool stripCustom = false)
            {
                Options = options;
                if (!string.IsNullOrWhiteSpace(options.Option1))
                {
                    Id1 = (int)options.Option1Effect;
                    Name1 = stripCustom && options.Option1.StartsWith("custom_") ? options.Option1["custom_".Length..] : options.Option1;
                }
                if (!string.IsNullOrWhiteSpace(options.Option2))
                {
                    Id2 = (int)options.Option2Effect;
                    Name2 = stripCustom && options.Option2.StartsWith("custom_") ? options.Option2["custom_".Length..] : options.Option2;
                }
                if (!string.IsNullOrWhiteSpace(options.Option3))
                {
                    Id3 = (int)options.Option3Effect;
                    Name3 = stripCustom && options.Option3.StartsWith("custom_") ? options.Option3["custom_".Length..] : options.Option3;
                }
            }

            public bool Equals(UpgradeOption other)
            {
                return Id1 == other.Id1 && Id2 == other.Id2 && Id3 == other.Id3;
            }

            public override bool Equals(object obj)
            {
                return obj is UpgradeOption effect && Equals(effect);
            }

            public static bool operator ==(UpgradeOption left, UpgradeOption right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(UpgradeOption left, UpgradeOption right)
            {
                return !(left == right);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Id1, Id2, Id3);
            }
        }
    }
}
