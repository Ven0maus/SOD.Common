using BepInEx.Configuration;
using System;
using System.Collections.Generic;

namespace SOD.Plugins.Common.BepInEx
{
    public class ConfigBuilder
    {
        private readonly Dictionary<string, Dictionary<string, object>> _configuration = new(StringComparer.OrdinalIgnoreCase);

        public ConfigEntryBase this[string section, string key]
        {
            get { return Get<ConfigEntryBase>(section, key); }
        }

        public ConfigEntryBase this[string identifier]
        {
            get
            {
                var (section, key) = SplitIdentifier(identifier);
                return Get<ConfigEntryBase>(section, key);
            }
        }

        /// <summary>
        /// The base configuration file, can be used to modify certain settings.
        /// </summary>
        public ConfigFile File { get; }

        public ConfigBuilder(ConfigFile config)
        {
            File = config;
        }

        public ConfigBuilder Add<T>(string identifier, T defaultValue = default, string description = null)
        {
            var (section, key) = SplitIdentifier(identifier);
            return Add(section, key, defaultValue, description);
        }

        public ConfigBuilder Add<T>(string section, string key, T defaultValue = default, string description = null)
        {
            if (!_configuration.TryGetValue(section, out var entries))
                _configuration.Add(section, entries = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase));

            if (!entries.ContainsKey(key))
                entries.Add(key, File.Bind(section, key, defaultValue, description));
            return this;
        }

        public T Get<T>(string identifier)
        {
            var (section, key) = SplitIdentifier(identifier);
            return Get<T>(section, key);
        }

        public T Get<T>(string section, string key)
        {
            if (_configuration.TryGetValue(section, out var entries) && entries.TryGetValue(key, out var entry))
            {
                if (typeof(T) == typeof(ConfigEntryBase))
                    return (T)entry;
                else
                    return ((ConfigEntry<T>)entry).Value;
            }
            return default;
        }

        public void Set<T>(string identifier, T value)
        {
            var (section, key) = SplitIdentifier(identifier);
            Set(section, key, value);
        }

        public void Set<T>(string section, string key, T value)
        {
            if (ExistsInternal<T>(section, key, out var entry))
                entry.BoxedValue = value;
            else
                throw new Exception($"No configuration entry exists for section \"{section}\" and key \"{key}\".");
        }

        public bool Exists<T>(string identifier, out T entry)
        {
            var exists = ExistsInternal<T>(identifier, out var config);
            entry = exists ? config.Value : default;
            return exists;
        }

        public bool Exists<T>(string section, string key, out T entry)
        {
            var exists = ExistsInternal<T>(section, key, out var config);
            entry = exists ? config.Value : default;
            return exists;
        }

        private bool ExistsInternal<T>(string identifier, out ConfigEntry<T> config)
        {
            var (section, key) = SplitIdentifier(identifier);
            return ExistsInternal(section, key, out config);
        }

        private bool ExistsInternal<T>(string section, string key, out ConfigEntry<T> config)
        {
            config = null;
            if (_configuration.TryGetValue(section, out var entries) && entries.TryGetValue(key, out var entry))
            {
                config = (ConfigEntry<T>)entry;
                return true;
            }
            return false;
        }

        private static (string section, string key) SplitIdentifier(string identifier)
        {
            var parts = identifier.Split('.');
            if (parts.Length != 2)
                throw new Exception($"Invalid configuration identifier \"{identifier}\" provided.");
            return (parts[0], parts[1]);
        }
    }

    public static class ConfigEntryBaseExtensions
    {
        public static T Value<T>(this ConfigEntryBase entryBase)
        {
            return (T)entryBase.BoxedValue;
        }
    }
}
