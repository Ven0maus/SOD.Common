using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using SOD.Common.BepInEx.Configuration;
using SOD.Common.BepInEx.Proxies;
using SOD.Common.Extensions.Internal;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SOD.Common.BepInEx
{
    /// <summary>
    /// Placeholder interface for an empty bindings implementation
    /// </summary>
    public interface IEmptyBindings
    { }

    /// <summary>
    /// Base plugin controller with no configuration bindings
    /// </summary>
    public abstract class PluginController<TImpl> : PluginController<TImpl, IEmptyBindings>
        where TImpl : PluginController<TImpl>
    {
        /// <summary>
        /// The original configuration implementation provided by BepInEx.
        /// </summary>
        public new ConfigFile Config { get { return base.ConfigFile; } }

        [Obsolete("This property is unused for this implementation, use \"Config\" property instead.", true)]
        public new ConfigFile ConfigFile { get { return base.ConfigFile; } }
    }

    /// <summary>
    /// Base plugin controller with custom configuration bindings
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class PluginController<TImpl, TBindings> : BasePlugin
        where TImpl : PluginController<TImpl, TBindings>
        where TBindings : class
    {
        /// <summary>
        /// Provides an instance implementation for the current plugin.
        /// </summary>
        public static TImpl Instance { get; private set; }
        /// <summary>
        /// A model based configuration implementation.
        /// <br>The config file can be accessed through the property <see cref="ConfigFile"/>.</br>
        /// </summary>
        public new TBindings Config { get; }
        /// <summary>
        /// The original configuration implementation provided by BepInEx.
        /// </summary>
        public ConfigFile ConfigFile { get { return base.Config; } }
        /// <summary>
        /// Static log source
        /// </summary>
        public new static ManualLogSource Log { get; private set; }

        /// <summary>
        /// A harmony instance
        /// </summary>
        protected Harmony Harmony { get; }

        private string _pluginGUID;
        private string PluginGUID
        {
            get
            {
                if (_pluginGUID == null)
                {
                    var type = GetType();
                    var pluginAttribute = type.GetCustomAttribute<BepInPlugin>();
                    _pluginGUID = pluginAttribute.GUID;
                }
                return _pluginGUID;
            }
        }

        public PluginController()
        {
            if (Instance != null)
                throw new Exception("A PluginController instance already exist.");

            Instance = (TImpl)this;
            Log = base.Log;
            Harmony = new Harmony(PluginGUID);

            // There is no point in setting up empty bindings
            if (HasConfigurationBindings())
            {
                Config = ConfigurationProxy<TBindings>.Create(new ConfigBuilder(base.Config));
                Log.LogInfo($"Setting up configuration bindings.");
                OnConfigureBindings();
            }
        }

        /// <summary>
        /// Returns true/false if there are configuration bindings defined.
        /// </summary>
        /// <returns></returns>
        public bool HasConfigurationBindings()
        {
            var bindingsType = typeof(TBindings).ExpandInheritedInterfaces();
            return bindingsType.SelectMany(a => a.GetProperties()).Any();
        }

        /// <summary>
        /// This method is the entrypoint for the plugin.
        /// </summary>
        public override void Load()
        { }

        private bool _createdNewConfigFile = false;
        /// <summary>
        /// This method is used to setup configuration bindings.
        /// </summary>
        public virtual void OnConfigureBindings()
        {
            var configFileExists = File.Exists(ConfigFile.ConfigFilePath);
            ConfigFile.SaveOnConfigSet = false;
            // This accesses the proxy of each property which binds the configuration of that property
            var properties = Config.GetType().ExpandInheritedInterfaces().SelectMany(a => a.GetProperties());
            bool issuesDuringBindingValidation = false;
            foreach (var property in properties)
            {
                if (OnValidateBinding(property))
                    issuesDuringBindingValidation = true;
            }
            if (issuesDuringBindingValidation)
                throw new Exception("Invalid binding configuration.");
            // Do a save once after setting all config
            ConfigFile.Save();
            ConfigFile.SaveOnConfigSet = true;

            if (!configFileExists && File.Exists(ConfigFile.ConfigFilePath))
                _createdNewConfigFile = true;
        }

        /// <summary>
        /// Plugin exit point
        /// </summary>
        /// <returns></returns>
        public override bool Unload()
        {
            Harmony.UnpatchSelf();
            Log.LogInfo($"Plugin is unloaded.");
            return true;
        }

        /// <summary>
        /// When changes have happened in the config bindings, this will adjust the layout properly again.
        /// <br>Ideally this method is called in override of OnConfigureBindings after base method execution.</br>
        /// <br>Do not use this method when you have manually adjusted the ConfigFile using Bind.</br>
        /// </summary>
        public void UpdateConfigFileLayout()
        {
            try
            {
                if (_createdNewConfigFile || ConfigFile == null || string.IsNullOrWhiteSpace(ConfigFile.ConfigFilePath) || !File.Exists(ConfigFile.ConfigFilePath)) return;
                if (!HasConfigurationBindings())
                {
                    File.Delete(ConfigFile.ConfigFilePath);
                    Plugin.Log.LogInfo($"Deleted outdated configuration file for mod \"{PluginGUID}\".");
                    return;
                }
                if (Config == null) return;

                var helper = new ConfigHelper(ConfigFile.ConfigFilePath);
                var configEntries = helper.GetConfigEntries();
                var properties = Config
                    .GetType()
                    .ExpandInheritedInterfaces()
                    .SelectMany(a => a.GetProperties())
                    .Where(a => a.GetCustomAttribute<BindingAttribute>() != null)
                    .Select(a =>
                    {
                        var (section, key) = ConfigHelper.SplitIdentifier(a.GetCustomAttribute<BindingAttribute>().Name ?? "");
                        return new { Section = section, Key = key };
                    })
                    .GroupBy(a => a.Section)
                    .ToDictionary(a => a.Key, a => a.Select(b => b.Key).ToArray(), StringComparer.OrdinalIgnoreCase);

                foreach (var configEntry in configEntries)
                {
                    // If configEntry is not in properties remove it
                    var section = configEntry.Key;
                    if (properties.TryGetValue(section, out var validValues))
                    {
                        var invalidValues = configEntry.Value.Except(validValues ?? Array.Empty<string>());
                        foreach (var value in invalidValues)
                            helper.RemoveEntry(value, section);
                    }
                    else
                    {
                        helper.RemoveSection(section);
                    }
                }

                if (helper.IsModified)
                {
                    helper.Update();
                    Plugin.Log.LogInfo($"Updated outdated configuration file layout for mod \"{PluginGUID}\".");
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogInfo($"Caught an exception while attemping to clean up an outdated config file for mod \"{PluginGUID}\": {ex.Message}");
                Plugin.Log.LogInfo($"Manually delete the config file if you wish to generate a new one.");
            }
        }

        private bool OnValidateBinding(PropertyInfo info)
        {
            try
            {
                _ = info.GetValue(Config);
            }
            catch (Exception ex)
            {
                Log.LogError($"[Binding({info.Name})]: {ex.InnerException?.Message ?? ex.Message}");
                return true;
            }
            return false;
        }
    }
}
