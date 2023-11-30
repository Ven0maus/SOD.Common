namespace SOD.Plugins.LightsOutTruce
{
    public class PluginHooks
    {
        //[HarmonyPatch(typeof(Class), "Method")]
        public class Class_Method
        {
            public static void Prefix()
            {
                Plugin.Log.LogInfo("Executing \"Class_Method\"");

                // TODO: Execute logic

                Plugin.Log.LogInfo("Executed \"Class_Method\"");
            }
        }
    }
}
