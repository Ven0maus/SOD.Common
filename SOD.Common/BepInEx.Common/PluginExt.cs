using BepInEx.Unity.IL2CPP;

namespace SOD.Common.BepInEx.Common
{
    public abstract class PluginExt : BasePlugin
    {
        protected new ConfigBuilder Config;

        public PluginExt()
        {
            Config = new ConfigBuilder(base.Config);
        }
    }
}
