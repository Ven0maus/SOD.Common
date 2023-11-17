using BepInEx;
using SOD.Common.BepInEx.Common;

namespace SOD.Common
{
    [BepInPlugin(Identifier, Name, Version)]
    public class Plugin : PluginExt
    {
        public const string Identifier = "Venomaus.Plugins.SOD.Common";
        private const string Name = "SOD.Common";
        private const string Version = "1.0.0";

        public override void Load()
        {
            Log.LogInfo($"Loaded Plugin {Identifier}");
        }
    }
}