using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SOD.Common.Patches
{
    /// <summary>
    /// Adds color code support to logging.
    /// </summary>
    internal class BepInExLoggerPatches
    {
        // Map common names to ConsoleColor
        private static readonly Dictionary<string, ConsoleColor> _colorNameMapping = new(StringComparer.OrdinalIgnoreCase)
        {
            { "black", ConsoleColor.Black },
            { "darkblue", ConsoleColor.DarkBlue },
            { "darkgreen", ConsoleColor.DarkGreen },
            { "darkcyan", ConsoleColor.DarkCyan },
            { "darkred", ConsoleColor.DarkRed },
            { "darkmagenta", ConsoleColor.DarkMagenta },
            { "darkyellow", ConsoleColor.DarkYellow },
            { "gray", ConsoleColor.Gray },
            { "darkgray", ConsoleColor.DarkGray },
            { "blue", ConsoleColor.Blue },
            { "green", ConsoleColor.Green },
            { "cyan", ConsoleColor.Cyan },
            { "red", ConsoleColor.Red },
            { "magenta", ConsoleColor.Magenta },
            { "yellow", ConsoleColor.Yellow },
            { "white", ConsoleColor.White }
        };

        [HarmonyPatch(typeof(ConsoleLogListener), nameof(ConsoleLogListener.LogEvent))]
        internal static class ConsoleLogListener_LogEvent
        {
            [HarmonyPrefix]
            internal static bool Prefix(LogEventArgs eventArgs)
            {
                var textContent = eventArgs.ToStringLine();

                // TODO: Remove color tags from the logfile
                // More color variety support
                var parsedContent = ColorStringParser.Parse(textContent, eventArgs.Level.GetConsoleColor());
                foreach (var (color, text) in parsedContent)
                {
                    ConsoleManager.SetConsoleColor(color);
                    ConsoleManager.ConsoleStream?.Write(text);
                    ConsoleManager.SetConsoleColor(ConsoleColor.Gray);
                }

                return false;
            }
        }

        internal static class ColorStringParser
        {
            private static readonly Regex ColorTagRegex = new(@"<color=([a-zA-Z]+)>(.*?)</color>", RegexOptions.Compiled);

            /// <summary>
            /// Parses text with Unity-style color tags into a list of (ConsoleColor, text) tuples
            /// </summary>
            internal static List<(ConsoleColor color, string text)> Parse(string text, ConsoleColor defaultColor)
            {
                var result = new List<(ConsoleColor, string)>();
                if (string.IsNullOrEmpty(text)) return result;

                int lastIndex = 0;

                foreach (Match match in ColorTagRegex.Matches(text))
                {
                    // Add any plain text before the tag
                    if (match.Index > lastIndex)
                    {
                        string plain = text[lastIndex..match.Index];
                        result.Add((defaultColor, plain)); // default color
                    }

                    string colorPart = match.Groups[1].Value;
                    string innerText = match.Groups[2].Value;

                    var consoleColor = ParseColor(colorPart, defaultColor);
                    result.Add((consoleColor, innerText));

                    lastIndex = match.Index + match.Length;
                }

                // Add remaining text after last match
                if (lastIndex < text.Length)
                {
                    string remaining = text[lastIndex..];
                    result.Add((defaultColor, remaining));
                }

                return result;
            }
        }

        /// <summary>
        /// Convert either a named color or a hex color into ConsoleColor
        /// </summary>
        private static ConsoleColor ParseColor(string color, ConsoleColor defaultColor)
        {
            if (string.IsNullOrEmpty(color))
                return defaultColor;

            if (_colorNameMapping.TryGetValue(color, out var cc))
                return cc;

            return defaultColor;
        }
    }
}
