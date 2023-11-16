using BepInEx;
using BepInEx.Unity.IL2CPP;

namespace SyncDisksRebalanced
{
    [BepInPlugin(Id, Name, Version)]
    public class Plugin : BasePlugin
    {
        private const string Id = "Venomaus.Plugins.SyncDisksRebalanced";
        private const string Name = "SyncDisksRebalanced";
        private const string Version = "1.0.0";

        public Plugin()
        {
            // Init configuration
        }

        public override void Load()
        {
            Log.LogInfo($"Init loading phase.");



            Log.LogInfo($"Loading phase completed.");
        }
    }
}