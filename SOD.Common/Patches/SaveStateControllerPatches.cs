using HarmonyLib;
using SOD.Common.Shadows;

namespace SOD.Common.Patches
{
    internal class SaveStateControllerPatches
    {
        [HarmonyPatch(typeof(CityConstructor), nameof(CityConstructor.StartLoading))]
        internal class CityConstructor_StartLoading
        {
            private static bool _loaded = false;
            private static Il2CppSystem.IO.FileInfo _fileInfo;

            [HarmonyPrefix]
            internal static void Prefix(CityConstructor __instance)
            {
                if (!__instance.generateNew && RestartSafeController.Instance.loadSaveGame)
                {
                    // Overwrite time, so we initialize again
                    ClockControllerPatches.ClockController_Update.LastTime = null;
                    Lib.Time.InitializeTime(true);

                    _loaded = true;
                    _fileInfo = RestartSafeController.Instance.saveStateFileInfo;
                    string filePath = _fileInfo?.FullPath;
                    Lib.SaveGame.OnLoad(filePath, false);
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
                    Lib.SaveGame.OnLoad(filePath, true);
                }
            }
        }

        [HarmonyPatch(typeof(MainMenuController), nameof(MainMenuController.OnMenuComponentSwitchComplete))]
        internal class MainMenuController_OnMenuComponentSwitchComplete
        {
            // BUGFIX: LoadGame triggers loading process twice when loading a file from an ongoing game, this fixes it

            private static MainMenuController.Component? _previousComponentCompleted;
            private static bool _wasDirty = false;

            [HarmonyPrefix]
            internal static bool Prefix(MainMenuController __instance)
            {
                if (__instance.currentComponent == null || _previousComponentCompleted == null) return true;
                if (__instance.currentComponent.component == __instance.previousComponent) return true;
                if (__instance.currentComponent.component == _previousComponentCompleted && SessionData.Instance.dirtyScene == _wasDirty) return false;
                return true;
            }

            [HarmonyPostfix]
            internal static void Postfix(MainMenuController __instance)
            {
                _previousComponentCompleted = __instance.currentComponent?.component;
                _wasDirty = SessionData.Instance.dirtyScene;
            }
        }

        [HarmonyPatch(typeof(SaveStateController), nameof(SaveStateController.CaptureSaveStateAsync))]
        internal class SaveStateController_CaptureSaveStateAsync
        {
            [HarmonyPrefix]
            internal static void Prefix(string path)
            {
                // The game saves to sodb when compression is enabled
                // The path retrieved here by default is always .sod
                // Also the slashs are in the wrong direction here???
                if (path != null && Game.Instance.useSaveGameCompression && path.EndsWith(".sod"))
                    path += "b";

                // Fix slashes
                path = path.Replace('/', '\\');

                Lib.SaveGame.OnSave(path, false);
            }

            [HarmonyPostfix]
            internal static void Postfix(string path)
            {
                // The game saves to sodb when compression is enabled
                // The path retrieved here by default is always .sod
                // Also the slashs are in the wrong direction here???
                if (path != null && Game.Instance.useSaveGameCompression && path.EndsWith(".sod"))
                    path += "b";

                // Fix slashes
                path = path.Replace('/', '\\');

                Lib.SaveGame.OnSave(path, true);
            }
        }

        [HarmonyPatch(typeof(MainMenuController), nameof(MainMenuController.DeleteSave))]
        internal class MainMenuController_DeleteSave
        {
            private static bool _delete = false;
            private static Il2CppSystem.IO.FileInfo _fileInfo;

            [HarmonyPrefix]
            internal static void Prefix(MainMenuController __instance)
            {
                if (__instance.selectedSave != null && !__instance.selectedSave.isInternal && 
                    __instance.selectedSave.info != null && System.IO.File.Exists(__instance.selectedSave.info.FullName))
                {
                    _delete = true;
                    _fileInfo = __instance.selectedSave.info;
                    Lib.SaveGame.OnDelete(_fileInfo.FullName, false);
                }
            }

            [HarmonyPostfix]
            internal static void Postfix()
            {
                if (_delete)
                {
                    _delete = false;
                    var path = _fileInfo.FullPath;
                    _fileInfo = null;
                    Lib.SaveGame.OnDelete(path, true);
                }
            }
        }
    }
}
