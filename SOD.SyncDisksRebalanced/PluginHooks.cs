using HarmonyLib;
using SOD.Common.Harmony.Common;
using SyncDisksRebalanced;

namespace SOD.SyncDisksRebalanced
{
    public class PluginHooks
    {
        //[HarmonyPatch(typeof(Class), "Method")]
        public class Class_Method
        {
            public static void Prefix()
            {
                if (HookHelper.RunOnlyOnce("Class_Method_Prefix")) 
                    return;

                Plugin.Log.LogInfo("Patching \"Class_Method\"");

                // TODO: Execute logic

                Plugin.Log.LogInfo("Patched \"Class_Method\"");
            }
        }

        /* 
        Interesting points to checkout:

        - SyncDiskElementController
        - InstallButton method

        - UpgradesController -> component on: /GameController/Upgrades/
        - Upgrades property (these are all the disks)
        - Upgrade.preset.name -> seems to be disk upgrade name?
         
         */
    }
}
