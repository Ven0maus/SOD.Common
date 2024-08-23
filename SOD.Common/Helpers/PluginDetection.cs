using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using System;
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
        /// Check if a BepInEx plugin is loaded given the plugin GUID.
        /// </summary>
        /// <param name="pluginGuid">The GUID of the target plugin.</param>
        /// <returns>True if the plugin has been loaded, false otherwise.</returns>
        public bool IsPluginLoaded(string pluginGuid)
            => IL2CPPChainloader.Instance.Plugins.ContainsKey(pluginGuid);

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
        public bool TryGetPluginInfo(string pluginGuid, out PluginInfo info)
            => IL2CPPChainloader.Instance.Plugins.TryGetValue(pluginGuid, out info);

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
                throw new System.Collections.Generic.KeyNotFoundException($"Plugin GUID ({pluginGuid}) not found.");
            return info;
        }

        /// <summary>
        /// Returns the actual plugin class.
        /// </summary>
        /// <param name="pluginInfo"></param>
        /// <returns></returns>
        public BasePlugin GetPlugin(PluginInfo pluginInfo, bool throwExceptionOnNull = true)
        {
            var plugin = (BasePlugin)pluginInfo.Instance.TryCast(typeof(BasePlugin));
            if (throwExceptionOnNull && plugin == null)
                throw new Exception("Unable to retrieve plugin object based on pluginInfo.");
            return plugin;
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
            var plugin = GetPlugin(info, true);
            if (!plugin.Config.TryGetEntry<T>(section: section, key: key, entry: out var entry))
                throw new InvalidOperationException($"No configuration entry found for ({section}, {key}).");
            return entry.Value;
        }

        /// <summary>
        /// Sets a new value into the currently assigned value of specified config setting for a BepInEx plugin given its GUID.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pluginGuid"></param>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public void SetPluginConfigEntryValue<T>(string pluginGuid, string section, string key, T value)
        {
            var info = GetPluginInfo(pluginGuid);
            var plugin = GetPlugin(info, true);
            if (!plugin.Config.TryGetEntry<T>(section: section, key: key, entry: out var entry))
                throw new InvalidOperationException($"No configuration entry found for ({section}, {key}).");
            entry.Value = value;
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
            var plugin = GetPlugin(info, true);
            plugin.Config.SettingChanged += (_, args) => listener(args);
        }
    }
}