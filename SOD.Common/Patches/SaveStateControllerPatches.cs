using HarmonyLib;
using SOD.Common.Shadows;
using System.IO;

namespace SOD.Common.Patches
{
    internal class SaveStateControllerPatches
    {
        [HarmonyPatch(typeof(SaveStateController), nameof(SaveStateController.LoadSaveState))]
        internal class SaveStateController_LoadSaveState
        {
            [HarmonyPrefix]
            internal static void Prefix(StateSaveData __instance)
            {
                Lib.SaveGame.GameLoaded(__instance, false);
            }

            [HarmonyPostfix]
            internal static void Postfix(StateSaveData __instance)
            {
                Lib.SaveGame.GameLoaded(__instance, true);
            }
        }

        [HarmonyPatch(typeof(SaveStateController), nameof(SaveStateController.CaptureSaveStateAsync))]
        internal class SaveStateController_CaptureSaveStateAsync
        {
            [HarmonyPrefix]
            internal static void Prefix()
            {
                Lib.SaveGame.GameSaved(null, false);
            }

            [HarmonyPostfix]
            internal static void Postfix(ref Il2CppSystem.Threading.Tasks.Task __result, string path)
            {
                // The game saves to sodb when compression is enabled
                // The path by default is always .sod
                if (Game.Instance.useSaveGameCompression)
                    path += "b";

                // Define how we enter out main logic
                if (!__result.IsCompleted)
                {
                    var continueAction = Lib.Il2Cpp.ConvertDelegate<Il2CppSystem.Action<Il2CppSystem.Threading.Tasks.Task>>(
                        (Il2CppSystem.Threading.Tasks.Task task) =>
                        {
                            ContinueFromMainCall(path);
                        });
                    __result.ContinueWith(continueAction);
                }
                else
                {
                    ContinueFromMainCall(path);
                }
            }

            private static void ContinueFromMainCall(string path)
            {
                if (!File.Exists(path))
                {
                    Plugin.Log.LogWarning($"[SaveGame-Provider] Could not load savedata file at path \"{path}\".");
                    return;
                }

                // Grab the save file and attempt to parse it into a valid StateSaveData object
                var fileInfo = new FileInfo(path);
                var onCompleteDataLoad = Lib.Il2Cpp.ConvertDelegate<Il2CppSystem.Action<StateSaveData>>(OnComplete);

                // Load StateSaveData from file path
                var task = DataCompressionController.Instance.LoadCompressedDataAsync(path, onCompleteDataLoad);
                if (!task.IsCompleted)
                {
                    var continueAction = Lib.Il2Cpp.ConvertDelegate<Il2CppSystem.Action<Il2CppSystem.Threading.Tasks.Task>>(
                        (Il2CppSystem.Threading.Tasks.Task t1) =>
                        {
                            if (!task.Result)
                                Plugin.Log.LogWarning("[SaveGame-Provider] Unable to load compressed data file.");
                        });
                    task.ContinueWith(continueAction);
                }
                else
                {
                    if (!task.Result)
                        Plugin.Log.LogWarning("[SaveGame-Provider] Unable to load compressed data file.");
                }
            }

            private static void OnComplete(StateSaveData stateSaveData)
            {
                Lib.SaveGame.GameSaved(stateSaveData, true);
            }
        }
    }
}
