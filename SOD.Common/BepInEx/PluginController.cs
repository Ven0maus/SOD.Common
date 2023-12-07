using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using SOD.Common.BepInEx.Configuration;
using SOD.Common.BepInEx.Proxies;
using SOD.Common.Extensions;
using System.Linq;
using System.Reflection;

namespace SOD.Common.BepInEx
{
    /// <summary>
    /// Base plugin controller with the default configuration bindings
    /// </summary>
    public abstract class PluginController : PluginController<IConfigurationBindings>
    { }

    /// <summary>
    /// Base plugin controller with custom configuration bindings
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class PluginController<T> : BasePlugin
        where T : class, IConfigurationBindings
    {
        private Harmony _harmony;
        /// <summary>
        /// A harmony instance
        /// </summary>
        protected Harmony Harmony { get { return _harmony ??= new Harmony(PluginGUID); } }
        /// <summary>
        /// A model based configuration implementation
        /// </summary>
        protected new T Config { get; }
        /// <summary>
        /// A builder for configuration, the original BepInEx config can be accessed on the ConfigBuilder.File property.
        /// </summary>
        protected ConfigBuilder ConfigBuilder { get; }

        /// <summary>
        /// Static log source
        /// </summary>
        public new static ManualLogSource Log { get; private set; }

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
            Log = base.Log;
            ConfigBuilder = new ConfigBuilder(base.Config);
            Config = ConfigurationProxy<T>.Create(ConfigBuilder);
        }

        /// <summary>
        /// This method is the entrypoint for the plugin.
        /// </summary>
        public override void Load()
        {
            if (!Config.Enabled)
            {
                Log.LogInfo($"Plugin \"{PluginGUID}\" is disabled.");
                return;
            }

            Log.LogInfo($"Plugin \"{PluginGUID}\" is setting up configuration bindings.");
            OnConfigureBindings();

            Log.LogInfo($"Plugin \"{PluginGUID}\" is loading.");
            OnBeforePatching();

            Log.LogInfo($"Plugin \"{PluginGUID}\" is patching.");
            OnPatching();

            Log.LogInfo($"Plugin \"{PluginGUID}\" is loaded.");
        }

        /// <summary>
        /// This method is used to setup configuration bindings.
        /// <br>Set savebindings to true if you want the configuration to be stored in the plugin config file.</br>
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
        /// This method is called right before patching happens.
        /// </summary>
        public virtual void OnBeforePatching()
        { }

        /// <summary>
        /// This method is called after OnLoading to patch all hooks in the assembly.
        /// <br>If overrriden you can use the Harmony property to do patching yourself.</br>
        /// </summary>
        public virtual void OnPatching()
        {
            Harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        /// <summary>
        /// Plugin exit point
        /// </summary>
        /// <returns></returns>
        public override bool Unload()
        {
            Harmony.UnpatchSelf();
            Log.LogInfo($"Plugin \"{PluginGUID}\" is unloaded.");
            return true;
        }
    }
}
