using HarmonyLib;
using SOD.Common.Shadows;

namespace SOD.Common.Patches
{
    internal class SaveStateControllerPatches
    {
        [HarmonyPatch(typeof(CityConstructor), nameof(CityConstructor.StartLoading))]
        internal class CityConstructor_GenerateCityFromShareCode
        {
            private static bool _loaded = false;
            private static Il2CppSystem.IO.FileInfo _fileInfo;

            [HarmonyPrefix]
            internal static void Prefix(CityConstructor __instance)
            {
                if (!__instance.generateNew && RestartSafeController.Instance.loadSaveGame)
                {
                    _loaded = true;
                    _fileInfo = RestartSafeController.Instance.saveStateFileInfo;
                    string filePath = _fileInfo?.FullPath;
                    // The game saves to sodb when compression is enabled
                    // The path by default is always .sod
                    if (filePath != null && Game.Instance.useSaveGameCompression && filePath.EndsWith(".sod"))
                        filePath += "b";
                    Lib.SaveGame.GameLoaded(filePath, false);
                }
            }

            [HarmonyPostfix]
            internal static void Postfix()
            {
                if (_loaded)
                {
                    _loaded = false;
                    string filePath = _fileInfo?.FullPath;
                    _fileInfo = null;
                    // The game saves to sodb when compression is enabled
                    // The path by default is always .sod
                    if (filePath != null && Game.Instance.useSaveGameCompression && filePath.EndsWith(".sod"))
                        filePath += "b";
                    Lib.SaveGame.GameLoaded(filePath, true);
                }
            }
        }

        [HarmonyPatch(typeof(SaveStateController), nameof(SaveStateController.CaptureSaveStateAsync))]
        internal class SaveStateController_CaptureSaveStateAsync
        {
            [HarmonyPrefix]
            internal static void Prefix(string path)
            {
                // The game saves to sodb when compression is enabled
                // The path by default is always .sod
                if (Game.Instance.useSaveGameCompression)
                    path += "b";
                Lib.SaveGame.GameSaved(path, false);
            }

            [HarmonyPostfix]
            internal static void Postfix(string path)
            {
                // The game saves to sodb when compression is enabled
                // The path by default is always .sod
                if (Game.Instance.useSaveGameCompression && !path.EndsWith(".sod"))
                    path += "b";
                Lib.SaveGame.GameSaved(path, true);
            }
        }
    }
}
