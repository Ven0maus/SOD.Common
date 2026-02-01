using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using UnityEngine;

namespace SOD.Common.Patches
{
    internal static class ModLoaderPatches
    {
        /// <summary>
        /// This property specifies if the mod menu was opened.
        /// </summary>
        private static bool _openedModMenu = false;

        /// <summary>
        /// The mod source we use to determine if its a bepinex plugin mod.
        /// </summary>
        private static readonly ModSettingsData.ModSource _bepinexPluginModSource = (ModSettingsData.ModSource)Enum.GetValues<ModSettingsData.ModSource>()
                    .Select(a => (int)a)
                    .Max() + 1;

        /// <summary>
        /// The custom bepinex icon we use to display the mod.
        /// </summary>
        private static readonly Lazy<Sprite> _bepinexIconSprite = new(() => CreateBepInExIconSprite());

        [HarmonyPatch(typeof(MainMenuController), nameof(MainMenuController.OnOpenModMenu))]
        internal static class MainMenuController_OnOpenModMenu
        {
            [HarmonyPrefix]
            private static void Prefix()
            {
                // We track it here, because GetModsComplete is called multiple times
                _openedModMenu = true;
            }
        }

        [HarmonyPatch(typeof(ModLoader), nameof(ModLoader.GetModsComplete))]
        internal static class ModLoader_GetModsComplete
        {
            // cached mod-list
            private static List<ModSettingsData> _modSettingsData;

            [HarmonyPrefix]
            private static void Prefix(ModLoader __instance)
            {
                if (!_openedModMenu) return;
                _openedModMenu = false;

                if (_modSettingsData != null)
                {
                    foreach (var mod in _modSettingsData)
                        __instance.modsListTemp.Add(mod);
                    return;
                }

                _modSettingsData = [];

                // Display all loaded bepinex plugins
                var bepinexPlugins = Lib.PluginDetection.GetAllLoadedPluginInfos();
                foreach (var plugin in bepinexPlugins)
                {
                    var metadata = plugin.Value.Metadata;
                    var location = plugin.Value.Location;

                    // Try to determine the thunderstore manifest json file
                    var manifestFilePath = Path.Combine(Path.GetDirectoryName(location), "manifest.json");
                    if (!File.Exists(manifestFilePath))
                    {
                        Plugin.Log.LogWarning($"Could not find manifest.json file of plugin \"{metadata.Name} | GUID: {metadata.GUID}\".");
                        continue;
                    }

                    ThunderstoreManifest manifest;
                    try
                    {
                        manifest = JsonSerializer.Deserialize<ThunderstoreManifest>(File.ReadAllText(manifestFilePath));
                    }
                    catch (Exception)
                    {
                        Plugin.Log.LogWarning($"Could not deserialize manifest.json file of plugin \"{metadata.Name} | GUID: {metadata.GUID}\".");
                        continue;
                    }

                    var version = metadata.Version > new SemanticVersioning.Version(manifest.version_number) ?
                            metadata.Version.ToString() : manifest.version_number;

                    var mod = new ModSettingsData
                    {
                        creator = manifest.author,
                        directory = null,
                        enabled = true,
                        modSource = _bepinexPluginModSource,
                        name = metadata.Name,
                        summary = version,
                        version = version,
                    };

                    _modSettingsData.Add(mod);
                    __instance.modsListTemp.Add(mod);
                }
            }
        }

        class ThunderstoreManifest
        {
            public string name { get; set; }
            public string version_number { get; set; }
            public string website_url { get; set; }
            public string description { get; set; }
            public string author { get; set; }
            public List<string> dependencies { get; set; }
        }

        [HarmonyPatch(typeof(ModEntryController), nameof(ModEntryController.StateRefresh))]
        internal static class ModEntryController_StateRefresh
        {
            [HarmonyPostfix]
            private static void Postfix(ModEntryController __instance)
            {
                if (__instance.mod.modSource == _bepinexPluginModSource)
                {
                    __instance.iconImg.sprite = _bepinexIconSprite.Value;
                    __instance.tooltip.mainDictionaryKey = "bepinex plugin";

                    if (__instance.enableDisableButton.gameObject.activeSelf)
                        __instance.enableDisableButton.gameObject.SetActive(false);
                    if (__instance.moveDownButton.gameObject.activeSelf)
                        __instance.moveDownButton.gameObject.SetActive(false);
                    if (__instance.moveUpButton.gameObject.activeSelf)
                        __instance.moveUpButton.gameObject.SetActive(false);
                }
            }
        }

        [HarmonyPatch(typeof(ModSettingsData), nameof(ModSettingsData.SaveSettings))]
        internal static class ModSettingsData_SaveSettings
        {
            [HarmonyPrefix]
            private static bool Prefix(ModSettingsData __instance)
            {
                if (__instance.modSource == _bepinexPluginModSource)
                {
                    return false;
                }
                return true;
            }
        }

        private static Sprite CreateBepInExIconSprite()
        {
            var filePath = Lib.SaveGame.GetPluginDataPath(Assembly.GetExecutingAssembly(), "bepinex_icon.png");
            if (!File.Exists(filePath))
            {
                Plugin.Log.LogWarning("Unable to find icon image file at path: " + filePath);
                return null;
            }

            byte[] fileData = File.ReadAllBytes(filePath);

            Texture2D texture = new(48, 48, TextureFormat.RGBA32, false);
            if (!texture.LoadImage(fileData))
            {
                Plugin.Log.LogError("Failed to load icon image data from path: " + filePath);
                return null;
            }

            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.Apply();

            return Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                100
            );
        }
    }
}
