using BepInEx.Configuration;
using SOD.Common.BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SOD.Common.Extensions
{
    public static class BepInExExtensions
    {
        /// <summary>
        /// A non-generic bind method for bepinex configuration, uses reflection.
        /// </summary>
        /// <param name="configFile"></param>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <param name="description"></param>
        /// <param name="valueType"></param>
        /// <returns></returns>
        public static ConfigEntry Bind(this ConfigFile configFile, object defaultValue, string section, string key, string description)
        {
            var valueType = defaultValue.GetType();
            if (!TomlTypeConverter.CanConvert(valueType))
            {
                throw new ArgumentException(string.Format("Type {0} is not supported by the config system. Supported types: {1}", valueType, string.Join(", ", 
                    (from x in TomlTypeConverter.GetSupportedTypes() select x.Name).ToArray())));
            }

            var type = configFile.GetType();
            var ioLockField = type.GetField("_ioLock", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var ioLock = ioLockField.GetValue(configFile);

            lock (ioLock)
            {
                var configDefinition = new ConfigDefinition(section, key);
                var configDescription = !string.IsNullOrEmpty(description) ? new ConfigDescription(description) : null;
                var entriesProperty = type.GetProperty("Entries", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var entries = (Dictionary<ConfigDefinition, ConfigEntryBase>)entriesProperty.GetValue(configFile);

                if (entries.TryGetValue(configDefinition, out var value))
                {
                    return (ConfigEntry)value;
                }

                var configEntry = new ConfigEntry(configFile, configDefinition, valueType, defaultValue, configDescription);
                entries[configDefinition] = configEntry;

                var orphanedEntriesProperty = type.GetProperty("OrphanedEntries", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var orphanedEntries = (Dictionary<ConfigDefinition, string>)orphanedEntriesProperty.GetValue(configFile);
                if (orphanedEntries.TryGetValue(configDefinition, out var value2))
                {
                    configEntry.SetSerializedValue(value2);
                    orphanedEntries.Remove(configDefinition);
                }

                if (configFile.SaveOnConfigSet)
                {
                    configFile.Save();
                }

                return configEntry;
            }
        }
    }
}
