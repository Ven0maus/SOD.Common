using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using System.Reflection;

namespace SOD.Plugins.Common.BepInEx
{
    public abstract class PluginExt : BasePlugin
    {
        private Harmony _harmony;
        protected Harmony Harmony { get { return _harmony ??= new Harmony(Plugin_GUID); } }
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
        protected abstract string Plugin_GUID { get; }

        public new static ManualLogSource Log { get; private set; }

        public PluginExt()
        {
            Log = base.Log;
            Config = new ConfigBuilder(base.Config);
        }

        /// <summary>
        /// This method is used to setup configuration bindings, it happens before plugin enabled check.
        /// </summary>
        public virtual void Configuration()
        { }

        /// <summary>
        /// This method is the entrypoint for the plugin.
        /// </summary>
        public override void Load()
        {
            Config.File.SaveOnConfigSet = false;
            Configuration();
            Config.File.Save();
            Config.File.SaveOnConfigSet = true;

            if (!Plugin_Enabled)
            {
                Log.LogInfo($"Plugin \"{Plugin_GUID}\" is disabled.");
                return;
            }

            Log.LogInfo($"Plugin \"{Plugin_GUID}\" is loaded.");

            Patch();
        }

        /// <summary>
        /// Used to patch harmony hooks, override to do manual patching.
        /// <br>Use the Harmony property to do patching.</br>
        /// </summary>
        public virtual void Patch()
        {
            Harmony.PatchAll(Assembly.GetExecutingAssembly());
            Log.LogInfo($"Plugin \"{Plugin_GUID}\" is patched.");
        }

        /// <summary>
        /// Method called when plugin is unloaded.
        /// </summary>
        /// <returns></returns>
        public override bool Unload()
        {
            Harmony.UnpatchSelf();
            Log.LogInfo($"Plugin \"{Plugin_GUID}\" unpatched.");
            Log.LogInfo($"Plugin \"{Plugin_GUID}\" unloaded.");
            return true;
        }
    }
}
