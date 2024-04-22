using System;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using UniverseLib;

namespace SOD.Common.Helpers
{
    public sealed class PluginDetection
    {
        public event EventHandler OnAllPluginsFinishedLoading;
        public bool AllPluginsFinishedLoading { get; private set; } = false;
        private bool _initialized = false;

        public PluginDetection()
        {
            Initialize();
        }

        internal void Initialize()
        {
            if (_initialized)
            {
                return;
            }
            _initialized = true;

            var invokeEventAction = () => OnAllPluginsFinishedLoading?.Invoke(this, EventArgs.Empty);
            IL2CPPChainloader.Instance.Finished += invokeEventAction;
            IL2CPPChainloader.Instance.Finished += () => { AllPluginsFinishedLoading = true; };
        }

        /// <summary>
        /// Get the full GUID of a plugin by searching for a partial GUID
        /// across all loaded plugins.
        /// </summary>
        /// <remarks>
        /// Should be called in response to Lib.BepInEx.OnAllPluginsFinishedLoading.
        /// Useful for plugins such as Babbler, which use a prefix on their
        /// GUID to influence load order.
        /// </remarks>
        /// <param name="partialPluginGuid">A partial GUID.</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException"></exception>
        public string GetPluginGuidFromPartialGuid(string partialPluginGuid)
        {
            var guids = IL2CPPChainloader.Instance.Plugins.Keys;
            var matches = guids.Where(guid => guid.ToLower().Contains(partialPluginGuid.ToLower()));
            int matchCount = matches.Count();
            if (matchCount == 0)
            {
                return null;
            }
            if (matchCount > 1)
            {
                throw new System.InvalidOperationException($"Multiple GUIDs match the provided partial GUID ({partialPluginGuid}): {string.Join(", ", matches)}.");
            }
            return matches.First();
        }

        /// <summary>
        /// Check if a BepInEx plugin is loaded given the plugin GUID.
        /// </summary>
        /// <param name="pluginGuid">The GUID of the target plugin.</param>
        /// <returns>True if the plugin has been loaded, false otherwise.</returns>
        public bool IsPluginLoaded(string pluginGuid) => IL2CPPChainloader.Instance.Plugins.ContainsKey(pluginGuid);

        /// <summary>
        /// Gets the plugin info (incompatibilities, version, user-friendly
        /// name, etc.) of a BepInEx plugin given its GUID.
        /// </summary>
        /// <remarks>
        /// Should be called in response to Lib.BepInEx.OnAllPluginsFinishedLoading.
        /// </remarks>
        /// <param name="pluginGuid">The GUID of the target plugin.</param>
        /// <returns>The plugin's info.</returns>
        public PluginInfo GetPluginInfo(string pluginGuid) => IL2CPPChainloader.Instance.Plugins[pluginGuid];

        /// <summary>
        /// Gets the currently assigned value of a specified config setting for
        /// a BepInEx plugin given its GUID.
        /// </summary>
        /// <remarks>
        /// Should be called in response to Lib.BepInEx.OnAllPluginsFinishedLoading.
        /// </remarks>
        /// <typeparam name="T">The config entry value type.</typeparam>
        /// <param name="pluginGuid">The GUID of the target plugin.</param>
        /// <param name="section">The section of the config file where the
        /// target config setting resides.</param>
        /// <param name="key">The name of the target config setting.</param>
        /// <returns>The current value of the target config setting.</returns>
        /// <exception cref="System.InvalidOperationException"></exception>
        public T GetPluginConfigEntryValue<T>(string pluginGuid, string section, string key)
        {
            var info = GetPluginInfo(pluginGuid);
            var plugin = (BasePlugin)info.Instance.TryCast(typeof(BasePlugin));
            if (!plugin.Config.TryGetEntry<T>(section: section, key: key, entry: out var entry))
            {
                throw new System.InvalidOperationException($"No configuration entry found for ({section}, {key}).");
            }
            return entry.Value;
        }

        /// <summary>
        /// Adds a listener to the SettingChanged event handler of a BepInEx
        /// plugin's configuration manager given its GUID. Allows plugins to
        /// respond to changes in the config settings of other plugins.
        /// </summary>
        /// <remarks>
        /// Should be called in response to Lib.BepInEx.OnAllPluginsFinishedLoading.
        /// </remarks>
        /// <param name="pluginGuid">The GUID of the target plugin.</param>
        /// <param name="listener">The action to be called in response to a
        /// change in target plugin config settings.</param>
        public void AddPluginConfigEntryChangedListener(string pluginGuid, Action<SettingChangedEventArgs> listener)
        {
            var info = GetPluginInfo(pluginGuid);
            var plugin = (BasePlugin)info.Instance.TryCast(typeof(BasePlugin));
            plugin.Config.SettingChanged += (_, args) => listener(args);
        }
    }
}