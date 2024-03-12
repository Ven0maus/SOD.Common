using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SOD.Common.BepInEx.Configuration
{
    /// <summary>
    /// A class to easily make changes to the bepinex config file.
    /// </summary>
    internal sealed class ConfigHelper
    {
        private readonly string _filePath;
        private readonly List<string> _fileInfo = new();
        private readonly Dictionary<string, List<ConfigSetting>> _configEntries = new(StringComparer.OrdinalIgnoreCase);

        public bool IsModified { get; private set; } = false;

        internal ConfigHelper(string filePath)
        {
            _filePath = filePath;

            string[] lines = File.ReadAllLines(_filePath);
            string currentSection = null;
            ConfigSetting currentSetting = null;

            foreach (string line in lines)
            {
                var trimLine = line.Trim();
                if (currentSection == null && trimLine.StartsWith("##"))
                {
                    _fileInfo.Add(trimLine);
                }
                else if (trimLine.StartsWith("[") && trimLine.EndsWith("]"))
                {
                    currentSection = line.TrimStart('[').TrimEnd(']');
                    _configEntries.Add(currentSection, new List<ConfigSetting>());
                }
                else if (currentSetting == null && trimLine.StartsWith("##"))
                {
                    currentSetting = new ConfigSetting
                    {
                        Description = trimLine[2..].Trim()
                    };
                }
                else if (currentSetting != null && trimLine.StartsWith("##"))
                {
                    // More description
                    currentSetting.Description += '\n' + trimLine[2..].Trim();
                }
                else if (currentSetting != null && trimLine.StartsWith("#"))
                {
                    if (trimLine.StartsWith("# Setting type: "))
                        currentSetting.Type = trimLine["# Setting type: ".Length..].Trim();
                    else if (trimLine.StartsWith("# Default value: "))
                        currentSetting.DefaultValue = trimLine["# Default value: ".Length..].Trim();
                    else if (trimLine.StartsWith("# Acceptable values: "))
                    {
                        currentSetting.AcceptableValues = new List<string>();
                        var values = trimLine["# Acceptable values: ".Length..].Trim().Split(',');
                        foreach (var value in values)
                            currentSetting.AcceptableValues.Add(value.Trim());
                    }
                }
                else if (currentSetting != null && !string.IsNullOrWhiteSpace(line))
                {
                    var split = line.Split('=');
                    currentSetting.Name = split[0].Trim();
                    currentSetting.Value = split[1].Trim();
                }
                else if (currentSetting != null && string.IsNullOrWhiteSpace(line))
                {
                    _configEntries[currentSection].Add(currentSetting);
                    currentSetting = null;
                }
            }
        }

        internal IReadOnlyDictionary<string, string[]> GetConfigEntries()
        {
            return _configEntries.ToDictionary(a => a.Key, a => a.Value.Select(b => b.Name).ToArray());
        }

        internal void RemoveSection(string section)
        {
            if (_configEntries.Remove(section))
                IsModified = true;
        }

        internal static (string section, string key) SplitIdentifier(string identifier)
        {
            var parts = identifier.Split('.');
            if (parts.Length < 2 || parts.Any(string.IsNullOrWhiteSpace))
                throw new Exception($"Invalid configuration identifier format \"{identifier}\" provided, must be in format \"section.propertyName\".");

            // Support for identifiers such as "General.Sub.PropertyName"
            // If we have more than two parts, we concat all parts together except the last to use as the section
            string section = null;
            if (parts.Length > 2)
            {
                for (int i = 0; i < parts.Length - 1; i++)
                {
                    section += parts[i];
                    if (i < parts.Length - 2)
                        section += ".";
                }
            }
            else
            {
                section = parts[0];
            }
            return (section, parts.Last());
        }

        internal void RemoveEntry(string name, string section = null)
        {
            if (!string.IsNullOrWhiteSpace(section) && _configEntries.TryGetValue(section, out var configEntries))
            {
                if (configEntries.RemoveAll(a => a.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) > 0)
                    IsModified = true;
                if (configEntries.Count == 0)
                    if (_configEntries.Remove(section))
                        IsModified = true;
            }
            else
            {
                var toBeRemoved = new List<string>();
                foreach (var kvp in _configEntries)
                {
                    var list = kvp.Value;
                    if (list.RemoveAll(a => a.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) > 0)
                        IsModified = true;
                    if (list.Count == 0)
                        toBeRemoved.Add(kvp.Key);
                }
                foreach (var key in toBeRemoved)
                    if (_configEntries.Remove(key))
                        IsModified = true;
            }
        }

        internal void Update()
        {
            if (_configEntries.Count == 0 && File.Exists(_filePath))
            {
                File.Delete(_filePath);
                return;
            }

            if (!IsModified) return;

            using StreamWriter writer = new(_filePath, false);

            // Write file info
            foreach (var value in _fileInfo)
                writer.WriteLine(value);
            writer.WriteLine();

            // Write all config entries and sections
            foreach (var kvp in _configEntries)
            {
                writer.WriteLine($"[{kvp.Key}]");
                writer.WriteLine();

                foreach (var setting in kvp.Value)
                {
                    var descriptions = setting.Description.Split('\n');
                    foreach (var desc in descriptions)
                        writer.WriteLine($"## {desc}");

                    writer.WriteLine($"# Setting type: {setting.Type}");
                    writer.WriteLine($"# Default value: {setting.DefaultValue}");

                    if (setting.AcceptableValues != null && setting.AcceptableValues.Count > 0)
                        writer.WriteLine($"# Acceptable values: {string.Join(", ", setting.AcceptableValues)}");

                    writer.WriteLine($"{setting.Name} = {setting.Value}");
                    writer.WriteLine();
                }
            }

            IsModified = false;
        }

        internal class ConfigSetting
        {
            public string Description { get; set; }
            public string Type { get; set; }
            public string DefaultValue { get; set; }
            public List<string> AcceptableValues { get; set; }
            public string Name { get; set; }
            public string Value { get; set; }
        }
    }
}
