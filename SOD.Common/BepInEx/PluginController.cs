using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using SOD.Common.BepInEx.Configuration;
using SOD.Common.BepInEx.Proxies;
using System.Reflection;

namespace SOD.Common.BepInEx
{
    public abstract class PluginController : PluginController<IConfigurationBindings>
    { }

    public abstract class PluginController<T> : BasePlugin
        where T : class, IConfigurationBindings
    {
        private Harmony _harmony;
        protected Harmony Harmony { get { return _harmony ??= new Harmony(PluginGUID); } }
        protected new T Config { get; }
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

        public new static ManualLogSource Log { get; private set; }

        public PluginController()
        {
            Log = base.Log;
            ConfigBuilder = new ConfigBuilder(base.Config);
            Config = ConfigurationProxy<T>.Create(ConfigBuilder);
        }

        /// <summary>
        /// This method is used to setup configuration bindings, it happens before plugin enabled check.
        /// <br>Set savebindings to true if you want the configuration to be stored in the plugin config file.</br>
        /// </summary>
        public virtual void OnConfigureBindings(out bool savebindings) => savebindings = false;

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

            Log.LogInfo($"Plugin \"{PluginGUID}\" is loading.");

            ConfigBuilder.File.SaveOnConfigSet = false;
            Log.LogInfo($"Plugin \"{PluginGUID}\" is setting up configuration bindings.");
            OnConfigureBindings(out var saveBindings);
            if (saveBindings) ConfigBuilder.File.Save();
            ConfigBuilder.File.SaveOnConfigSet = true;

            Log.LogInfo($"Plugin \"{PluginGUID}\" is patching.");
            OnPatching();

            Log.LogInfo($"Plugin \"{PluginGUID}\" is loaded.");
        }

        /// <summary>
        /// Used to patch harmony hooks, override to do manual patching.
        /// <br>Use the Harmony property to do patching.</br>
        /// </summary>
        public virtual void OnPatching()
        {
            Harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        /// <summary>
        /// Method called when plugin is unloaded.
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
