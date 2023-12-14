using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using SOD.Common.BepInEx.Configuration;
using SOD.Common.BepInEx.Proxies;
using SOD.Common.Extensions;
using System;
using System.Linq;
using System.Reflection;

namespace SOD.Common.BepInEx
{
    public interface IEmptyBindings 
    { }

    /// <summary>
    /// Base plugin controller with no configuration bindings
    /// </summary>
    public abstract class PluginController : PluginController<IEmptyBindings>
    {
        // Since this class uses no bindings, there is no need to init the proxy
        public override void OnConfigureBindings() { }
    }

    /// <summary>
    /// Base plugin controller with custom configuration bindings
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class PluginController<T> : BasePlugin
        where T : class
    {
        /// <summary>
        /// Provides an instance implementation for the current plugin.
        /// </summary>
        public static PluginController<T> Instance { get; private set; }
        /// <summary>
        /// A model based configuration implementation.
        /// <br>The config file can be accessed through the property <see cref="ConfigFile"/>.</br>
        /// </summary>
        public new T Config { get; }
        /// <summary>
        /// The original configuration implementation provided by BepInEx.
        /// </summary>
        public ConfigFile ConfigFile { get { return ConfigBuilder.File; } }
        /// <summary>
        /// Static log source
        /// </summary>
        public new static ManualLogSource Log { get; private set; }

        /// <summary>
        /// A harmony instance
        /// </summary>
        protected Harmony Harmony { get; }
        /// <summary>
        /// A builder for configuration, the original BepInEx config can be accessed on the ConfigBuilder.File property.
        /// </summary>
        protected ConfigBuilder ConfigBuilder { get; }

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

            Instance = this;
            Log = base.Log;
            ConfigBuilder = new ConfigBuilder(base.Config);
            Config = ConfigurationProxy<T>.Create(ConfigBuilder);
            Harmony = new Harmony(PluginGUID);

            // There is no point in setting up empty bindings
            if (typeof(T) != typeof(IEmptyBindings))
            {
                Log.LogInfo($"Setting up configuration bindings.");
                OnConfigureBindings();
            }
        }

        /// <summary>
        /// This method is the entrypoint for the plugin.
        /// </summary>
        public override void Load()
        { }

        /// <summary>
        /// This method is used to setup configuration bindings.
        /// </summary>
        public virtual void OnConfigureBindings()
        {
            ConfigBuilder.File.SaveOnConfigSet = false;
            // This accesses the proxy of each property which binds the configuration of that property
            var properties = Config.GetType().ExpandInheritedInterfaces().SelectMany(a => a.GetProperties());
            foreach (var property in properties)
                _ = property.GetValue(Config);
            // Do a save once after setting all config
            ConfigBuilder.File.Save();
            ConfigBuilder.File.SaveOnConfigSet = true;
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
    }
}
