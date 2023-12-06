using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using System.Reflection;

namespace SOD.Common.BepInEx
{
    public abstract class PluginController : BasePlugin
    {
        private Harmony _harmony;
        protected Harmony Harmony { get { return _harmony ??= new Harmony(PluginGUID); } }
        protected new ConfigBuilder Config { get; }
        protected virtual bool Plugin_Enabled
        {
            get 
            {
                if (!Config.Exists<bool>("General.Enabled", out var entry))
                {
                    Config.Add("General.Enabled", true, "Should the plugin be enabled?");
                    return Config["General.Enabled"].Value<bool>();
                }
                return entry;
            }
            set
            {
                if (!Config.Exists<bool>("General.Enabled", out _))
                {
                    Config.Add("General.Enabled", true, "Should the plugin be enabled?");
                    Config["General.Enabled"].BoxedValue = value;
                    return;
                }
                Config["General.Enabled"].BoxedValue = value;
            }
        }

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
            Config = new ConfigBuilder(base.Config);
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
            if (!Plugin_Enabled)
            {
                Log.LogInfo($"Plugin \"{PluginGUID}\" is disabled.");
                return;
            }

            Log.LogInfo($"Plugin \"{PluginGUID}\" is loading.");

            Config.File.SaveOnConfigSet = false;
            Log.LogInfo($"Plugin \"{PluginGUID}\" is setting up configuration bindings.");
            OnConfigureBindings(out var saveBindings);
            if (saveBindings) Config.File.Save();
            Config.File.SaveOnConfigSet = true;

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
