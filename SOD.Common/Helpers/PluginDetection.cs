using System;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using Il2CppSystem.Runtime.InteropServices;
using UniverseLib;

namespace SOD.Common.Helpers
{
    public sealed class PluginDetection
    {
        /// <summary>
        /// Raised right after the BepInEx chainloader finishes loading the
        /// last plugin.
        /// </summary>
        public event EventHandler OnAllPluginsFinishedLoading;

        /// <summary>
        /// True if the BepInEx chainloader has finished loading all plugins,
        /// false otherwise (is set to true after <c>OnAllPluginsFinishedLoading</c>).
        /// </summary>
        public bool AllPluginsFinishedLoading { get; private set; } = false;

        internal PluginDetection()
        {
            IL2CPPChainloader.Instance.Finished += OnAllPluginsFinishedLoadingListener;
        }

        private void OnAllPluginsFinishedLoadingListener()
        {
            AllPluginsFinishedLoading = true;
            OnAllPluginsFinishedLoading?.Invoke(this, EventArgs.Empty);
            IL2CPPChainloader.Instance.Finished -= OnAllPluginsFinishedLoadingListener;
        }

        /// <summary>
        /// Get the full GUID of a plugin by searching for a partial GUID
        /// across all loaded plugins.
        /// </summary>
        /// <remarks>
        /// Should be called in response to <c>OnAllPluginsFinishedLoading</c>.
        /// Useful for plugins such as Babbler, which use a prefix on their
        /// GUID to influence load order.
        /// </remarks>
        /// <param name="partialPluginGuid">A partial GUID.</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException"></exception>
        public string GetPluginGuidFromPartialGuid(string partialPluginGuid)
        {
            var guids = IL2CPPChainloader.Instance.Plugins.Keys;
            var matches = guids.Where(guid => guid.ToLower().Contains(partialPluginGuid.ToLower())).ToArray();
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
        /// Should be called in response to <c>OnAllPluginsFinishedLoading</c>.
        /// </remarks>
        /// <param name="pluginGuid">The GUID of the target plugin.</param>
        /// <param name="info">The BepInEx PluginInfo data object of the target
        /// plugin.</param>
        /// <returns></returns>
        public bool TryGetPluginInfo(string pluginGuid, out PluginInfo info) => IL2CPPChainloader.Instance.Plugins.TryGetValue(pluginGuid, out info);

        /// <summary>
        /// Gets the plugin info (incompatibilities, version, user-friendly
        /// name, etc.) of a BepInEx plugin given its GUID.
        /// </summary>
        /// <remarks>
        /// Should be called in response to <c>OnAllPluginsFinishedLoading</c>.
        /// </remarks>
        /// <param name="pluginGuid">The GUID of the target plugin.</param>
        /// <returns>The plugin's info.</returns>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException"></exception>
        public PluginInfo GetPluginInfo(string pluginGuid)
        {
            if (!TryGetPluginInfo(pluginGuid, out var info))
            {
                throw new System.Collections.Generic.KeyNotFoundException($"Plugin GUID ({pluginGuid}) not found.");
            }
            return info;
        }

        /// <summary>
        /// Gets the currently assigned value of a specified config setting for
        /// a BepInEx plugin given its GUID.
        /// </summary>
        /// <remarks>
        /// Should be called in response to <c>OnAllPluginsFinishedLoading</c>.
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
        /// Should be called in response to <c>OnAllPluginsFinishedLoading</c>.
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