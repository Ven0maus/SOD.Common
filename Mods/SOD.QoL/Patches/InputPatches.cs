using HarmonyLib;
using Rewired;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;

namespace SOD.QoL.Patches
{
    internal class InputPatches
    {
        [HarmonyPatch(typeof(InputController), nameof(InputController.Update))]
        internal class InputController_Update
        {
            // Some minor improvements for ways to exit menu and conversations

            [HarmonyPrefix]
            internal static bool Prefix(InputController __instance)
            {
                if (!Plugin.Instance.Config.EndConversationPatch && !Plugin.Instance.Config.UnpauseGameOnMainMenuExit) return true;
                if (!__instance.enabled || !ReInput.isReady) return true;

                if (__instance.player != null && __instance.player.GetButtonDown("Menu"))
                {
                    // This allows ending conversations with the menu key
                    if (Plugin.Instance.Config.EndConversationPatch &&
                        SessionData.Instance.startedGame && SessionData.Instance.play && !MainMenuController.Instance.mainMenuActive)
                    {
                        if (Player.Instance.interactingWith != null)
                        {
                            ActionController.Instance.Return(null, null, Player.Instance);
                            return false;
                        }
                    }

                    // This fixes the fact that the game is still paused after you exit the menu with the menu key
                    if (Plugin.Instance.Config.UnpauseGameOnMainMenuExit &&
                        SessionData.Instance.startedGame && !SessionData.Instance.play && MainMenuController.Instance.mainMenuActive &&
                        (CityConstructor.Instance == null || !CityConstructor.Instance.preSimActive))
                    {
                        SessionData.Instance.ResumeGame();
                        MainMenuController.Instance.EnableMainMenu(false, true, true, MainMenuController.Component.mainMenuButtons);
                        return false;
                    }
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(MapController), nameof(MapController.Setup))]
        public class MapController_Setup
        {
            // Changes the player color and size to be more visible on the map also zooms out entirely as default

            [HarmonyPrefix]
            public static void Prefix()
            {
                // Set marker color
                if (Plugin.Instance.Config.ChangePlayerMarkerColor)
                {
                    var playerMarkerColor = GetColor(Plugin.Instance.Config.PlayerMarkerColor);
                    PrefabControls.Instance.characterMarkerColor = playerMarkerColor;
                }

                // Enlarge marker
                if (Plugin.Instance.Config.EnlargePlayerMarker)
                {
                    var (x, y) = ExtractFloats(Plugin.Instance.Config.PlayerMarkerSize);
                    PrefabControls.Instance.playerMarker.transform.localScale = new Vector3(x, y, 1f);
                }
            }

            [HarmonyPostfix]
            public static void Postfix(MapController __instance)
            {
                if (!Plugin.Instance.Config.ZoomOutOnStart) return;

                // zoom all the way out as the default value
                float zoomLimitMin = __instance.zoomController.zoomLimit.x;
                __instance.zoomController.SetPivotPoint(0f, ZoomContent.ZoomPivot.playerMapPosition);
                __instance.zoomController.SetZoom(zoomLimitMin);
                __instance.zoomController.GetComponent<RectTransform>().localScale = new Vector3(zoomLimitMin, zoomLimitMin, 1f);
            }

            private static Color GetColor(string hexColor)
            {
                hexColor = hexColor.TrimStart('#');

                if (hexColor.Length != 6)
                {
                    Plugin.Log.LogWarning("Invalid hex color code in configuration, it should be in the format #RRGGBB. Falling back to default green color.");
                    hexColor = "008000";
                }

                int r = int.Parse(hexColor[..2], NumberStyles.HexNumber);
                int g = int.Parse(hexColor.Substring(2, 2), NumberStyles.HexNumber);
                int b = int.Parse(hexColor.Substring(4, 2), NumberStyles.HexNumber);

                return new Color(r, g, b);
            }

            private static (float x, float y) ExtractFloats(string input)
            {
                // Define a regular expression pattern for extracting floats
                string pattern = @"\((-?\d+(\.\d+)?), (-?\d+(\.\d+)?)\)";

                // Match the pattern in the input string
                Match match = Regex.Match(input, pattern);

                if (match.Success)
                {
                    // Parse the matched groups as floats
                    float firstFloat = float.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
                    float secondFloat = float.Parse(match.Groups[3].Value, CultureInfo.InvariantCulture);

                    // Return the result as a Tuple
                    return (firstFloat, secondFloat);
                }
                else
                {
                    // Default if input invalid
                    Plugin.Log.LogWarning("Invalid player marker size configuration, using the default value (2.5, 2.5)");
                    return (2.5f, 2.5f);
                }
            }
        }

        [HarmonyPatch(typeof(MapController), nameof(MapController.CentreOnObject))]
        public class MapController_CentreOnObject
        {
            // When zooming and moving around the camera, then centering on an object the camera doesn't move properly, this fixes that.

            private static bool _calledBefore = false;

            [HarmonyPostfix]
            public static void Postfix(MapController __instance, RectTransform mapObj, bool instant, bool showPointer)
            {
                if (!Plugin.Instance.Config.FixCenterOnPlayer) return;
                if (_calledBefore)
                {
                    _calledBefore = false;
                    return;
                }

                _calledBefore = true;

                // Call it again, it seems to fix itself with the zooming.
                __instance.CentreOnObject(mapObj, instant, showPointer);
            }
        }
    }
}
