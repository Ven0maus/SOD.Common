using HarmonyLib;

namespace SOD.ClumsyCitizens
{
    public class PluginHooks
    {
        [HarmonyPatch(typeof(NewAIGoal), nameof(NewAIGoal.InsertUnlockAction))]
        public class NewAIGoal_InsertUnlockAction
        {
            private static bool _hasAdjusted;
            public static void Prefix(NewDoor door, ref bool lockBehind)
            {
                Plugin.Log.LogInfo("Executing \"NewAIGoal_InsertUnlockAction_Prefix\"");

                // If the door is suppose to be locked behind, have an opportunity to be clumsy
                if (lockBehind)
                {
                    Plugin.Log.LogInfo($"Door locked state before postfix: " + (door.isLocked ? "locked" : "unlocked" + " | lockBehind: " + lockBehind));
                    if (Toolbox.Instance.Rand(0, 100, true) < Plugin.Instance.Config.ChanceToForgetDoorLock)
                        lockBehind = false;
                    _hasAdjusted = true;
                    Plugin.Log.LogInfo($"Left door({door.GetInstanceID()}) unlocked.");
                }

                Plugin.Log.LogInfo("Executed \"NewAIGoal_InsertUnlockAction_Prefix\"");
            }

            public static void Postfix(NewDoor door, bool lockBehind)
            {
                Plugin.Log.LogInfo("Executing \"NewAIGoal_InsertUnlockAction_Postfix\"");

                if (_hasAdjusted)
                {
                    _hasAdjusted = false;
                    Plugin.Log.LogInfo($"Door locked state after postfix: " + (door.isLocked ? "locked" : "unlocked" + " | lockBehind: " + lockBehind));
                }

                Plugin.Log.LogInfo("Executed \"NewAIGoal_InsertUnlockAction\"_Postfix");
            }
        }
    }
}
