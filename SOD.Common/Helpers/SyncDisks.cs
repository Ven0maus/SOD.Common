using SOD.Common.Helpers.SyncDiskObjects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Xml.Linq;

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

        internal readonly Dictionary<string, List<InstalledSyncDiskData>> InstalledSyncDisks = new();

        internal class InstalledSyncDiskData
        {
            public string SyncDiskName { get; set; }
            public string Effect { get; set; }
            public List<string> UpgradeOptions { get; set; }

            public InstalledSyncDiskData() { }

            internal InstalledSyncDiskData(string name, string effectName)
            {
                SyncDiskName = name;
                Effect = effectName;
                UpgradeOptions = new();
            }
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
        /// <param name="syncDiskName">The name of the sync disk</param>
        /// <param name="pluginGuid">The mod's plugin guid</param>
        /// <param name="reRaiseEventsOnSaveLoad">Should the install/upgrade events re-raise when a savefile is loaded that has this disk installed?</param>
        /// <returns></returns>
        public SyncDiskBuilder Builder(string syncDiskName, string pluginGuid, bool reRaiseEventsOnSaveLoad = true)
        {
            if (string.IsNullOrWhiteSpace(syncDiskName))
                throw new ArgumentException("Parameter cannot be empty or whitespace.", nameof(syncDiskName));
            if (string.IsNullOrWhiteSpace(pluginGuid))
                throw new ArgumentException("Parameter cannot be empty or whitespace.", nameof(pluginGuid));
            return new SyncDiskBuilder(syncDiskName, pluginGuid, reRaiseEventsOnSaveLoad);
        }

        internal void CheckForSyncDiskData(bool onLoad, string saveFilePath)
        {
            if (onLoad)
                InstalledSyncDisks.Clear();

            var hash = Lib.SaveGame.GetUniqueString(saveFilePath);
            var path = Lib.SaveGame.GetSavestoreDirectoryPath(Assembly.GetExecutingAssembly(), $"syncdiskdata_{hash}.json");
            if (onLoad && !File.Exists(path)) return;

            if (onLoad)
            {
                // On load, raise all correct events of installed effects and upgrades
                var json = File.ReadAllText(path);
                var data = JsonSerializer.Deserialize<SyncDiskJsonData>(json);
                if (data == null || data.SyncDiskData == null) return;

                foreach (var disk in data.SyncDiskData)
                {
                    // Raise correct events for each sync disk
                    var syncDisk = RegisteredSyncDisks.FirstOrDefault(a => a.Preset.name.Equals(disk.SyncDiskName));
                    // Get correct effect with right ID
                    var realEffect = syncDisk.Effects.First(a => a.Name.Equals(disk.Effect));
                    var index = Array.IndexOf(syncDisk.Effects, realEffect);
                    OnAfterSyncDiskInstalled?.Invoke(this, new SyncDiskArgs(syncDisk, realEffect));
                    Plugin.Log.LogInfo($"Loaded savegame, raised install event for custom disk: {disk.SyncDiskName} | {realEffect.Name}");

                    if (disk.UpgradeOptions != null)
                    {
                        // Options for the given effect index
                        var realOptions = syncDisk.UpgradeOptions[index];
                        foreach (var option in disk.UpgradeOptions)
                        {
                            // Get real option id
                            var realOption = realOptions.Name1 != null && realOptions.Name1.Equals(option) ? realOptions.Id1 :
                                realOptions.Name2 != null && realOptions.Name2.Equals(option) ? realOptions.Id2 : realOptions.Id3;
                            if (realOption == null) continue;
                            OnAfterSyncDiskUpgraded?.Invoke(this, new SyncDiskArgs(syncDisk, realEffect, new SyncDiskArgs.Option(realOption.Value, option)));
                            Plugin.Log.LogInfo($"Loaded savegame, raised upgrade event for disk: {disk.SyncDiskName} | {realEffect.Name} | {option}");
                        }
                    }
                    
                    if (!InstalledSyncDisks.TryGetValue(disk.SyncDiskName, out List<InstalledSyncDiskData> disks)) 
                    {
                        disks = new();
                        InstalledSyncDisks.Add(disk.SyncDiskName, disks);
                    }

                    disks.Add(disk);
                }
            }
            else
            {
                // Don't need to create a file if there are no custom sync disks installed
                if (InstalledSyncDisks.Count == 0)
                {
                    // If the file exists (from previous save, then delete it because there are no more custom installed disks in this save)
                    if (File.Exists(path))
                        File.Delete(path);
                    return;
                }

                // On save, serialize all sync disk data
                var toBeSaved = new SyncDiskJsonData() { SyncDiskData = InstalledSyncDisks.Values.SelectMany(a => a).OrderBy(a => a.SyncDiskName).ToList() };
                var json = JsonSerializer.Serialize(toBeSaved, new JsonSerializerOptions { WriteIndented = false });
                File.WriteAllText(path, json);

                Plugin.Log.LogInfo($"Saving game, writing custom sync disk data to common savestore.");
            }
        }

        internal class SyncDiskJsonData
        {
            public List<InstalledSyncDiskData> SyncDiskData { get; set; }
        }

        internal void RaiseSyncDiskEvent(SyncDiskEvent syncDiskEvent, bool after, UpgradesController.Upgrades upgrade)
        {
            switch (syncDiskEvent)
            {
                case SyncDiskEvent.OnInstall:
                    var installArgs = new SyncDiskArgs(upgrade, true, false);
                    if (after)
                        OnAfterSyncDiskInstalled?.Invoke(this, installArgs);
                    else
                        OnBeforeSyncDiskInstalled?.Invoke(this, installArgs);

                    // Add to dictionary that it is installed
                    if (after)
                    {
                        if (!installArgs.SyncDisk.Preset.name.StartsWith($"{SyncDisk.UniqueDiskIdentifier}_") || !installArgs.SyncDisk.ReRaiseEventsOnSaveLoad) break;
                        if (installArgs.Effect != null)
                        {
                            if (!InstalledSyncDisks.TryGetValue(installArgs.SyncDisk.Preset.name, out var disks1))
                            {
                                disks1 = new List<InstalledSyncDiskData>();
                                InstalledSyncDisks.Add(installArgs.SyncDisk.Preset.name, disks1);
                            }
                            disks1.Add(new InstalledSyncDiskData(installArgs.SyncDisk.Preset.name, installArgs.Effect.Value.Name));
                        }
                    }
                    break;
                case SyncDiskEvent.OnUninstall:
                    var uninstallArgs = new SyncDiskArgs(upgrade, true, false);
                    if (after)
                        OnAfterSyncDiskUninstalled?.Invoke(this, uninstallArgs);
                    else
                        OnBeforeSyncDiskUninstalled?.Invoke(this, uninstallArgs);

                    if (after)
                    {
                        // Remove from installed sync disks
                        if (uninstallArgs.Effect == null || !uninstallArgs.SyncDisk.Preset.name.StartsWith($"{SyncDisk.UniqueDiskIdentifier}_") || !uninstallArgs.SyncDisk.ReRaiseEventsOnSaveLoad) break;
                        if (!InstalledSyncDisks.TryGetValue(uninstallArgs.SyncDisk.Preset.name, out var disks2))
                        {
                            Plugin.Log.LogWarning($"Could not find uninstall data for custom sync disk \"{uninstallArgs.SyncDisk.Preset.name}\".");
                            break;
                        }

                        // Compare on name because ids might change when changing mod order, and this will be save in save file
                        var correctEffect = disks2.FirstOrDefault(a => a.Effect.Equals(uninstallArgs.Effect.Value.Name));
                        if (correctEffect == null)
                        {
                            Plugin.Log.LogWarning($"Could not find uninstall effect data for custom sync disk \"{uninstallArgs.SyncDisk.Preset.name}\".");
                            break;
                        }

                        disks2.Remove(correctEffect);
                        if (disks2.Count == 0)
                            InstalledSyncDisks.Remove(uninstallArgs.SyncDisk.Preset.name);
                    }
                    break;
                case SyncDiskEvent.OnUpgrade:
                    var upgradeArgs = new SyncDiskArgs(upgrade);
                    if (after)
                        OnAfterSyncDiskUpgraded?.Invoke(this, upgradeArgs);
                    else
                        OnBeforeSyncDiskUpgraded?.Invoke(this, upgradeArgs);

                    if (after)
                    {
                        if (!upgradeArgs.Effect.HasValue || !upgradeArgs.SyncDisk.Preset.name.StartsWith($"{SyncDisk.UniqueDiskIdentifier}_") || !upgradeArgs.SyncDisk.ReRaiseEventsOnSaveLoad) break;
                        if (!InstalledSyncDisks.TryGetValue(upgradeArgs.SyncDisk.Preset.name, out var disks3))
                        {
                            Plugin.Log.LogWarning($"Could not find upgrade data for custom sync disk \"{upgradeArgs.SyncDisk.Preset.name}\".");
                            break;
                        }

                        // Compare on name because ids might change when changing mod order, and this will be save in save file
                        var cEffect = disks3.FirstOrDefault(a => a.Effect.Equals(upgradeArgs.Effect.Value.Name));
                        if (upgradeArgs.UpgradeOption == null || cEffect == null)
                        {
                            Plugin.Log.LogWarning($"Could not find {(upgradeArgs.UpgradeOption == null ? "upgrade option" : "effect")} data for custom sync disk \"{upgradeArgs.SyncDisk.Preset.name}\".");
                            break;
                        }

                        cEffect.UpgradeOptions.Add(upgradeArgs.UpgradeOption.Value.Name);
                    }
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
            internal readonly string DdsIdentifier;

            internal Effect(int id, string name)
            {
                Id = id;
                Name = SyncDisk.GetName(name);
                DdsIdentifier = name;
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
            public readonly int? Id1, Id2, Id3;
            public readonly string Name1, Name2, Name3;

            internal readonly bool HasOptions => !string.IsNullOrWhiteSpace(Name1) || !string.IsNullOrWhiteSpace(Name2) || !string.IsNullOrWhiteSpace(Name3);
            internal readonly SyncDiskBuilder.Options Options;

            internal UpgradeOption(SyncDiskBuilder.Options options, bool stripCustom = false)
            {
                Options = options;
                if (!string.IsNullOrWhiteSpace(options.Option1))
                {
                    Id1 = (int)options.Option1Effect;
                    Name1 = stripCustom ? SyncDisk.GetName(options.Option1) : options.Option1;
                }
                if (!string.IsNullOrWhiteSpace(options.Option2))
                {
                    Id2 = (int)options.Option2Effect;
                    Name2 = stripCustom ? SyncDisk.GetName(options.Option2) : options.Option2;
                }
                if (!string.IsNullOrWhiteSpace(options.Option3))
                {
                    Id3 = (int)options.Option3Effect;
                    Name3 = stripCustom ? SyncDisk.GetName(options.Option3) : options.Option3;
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
