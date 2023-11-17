using BepInEx.Configuration;
using System;
using System.Collections.Generic;

namespace SOD.Common.BepInEx.Common
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

        private readonly ConfigFile _config;

        public ConfigBuilder(ConfigFile config)
        {
            _config = config;
        }

        public ConfigBuilder Add<T>(string identifier, T defaultValue = default)
        {
            var (section, key) = SplitIdentifier(identifier);
            return Add(section, key, defaultValue);
        }

        public ConfigBuilder Add<T>(string section, string key, T defaultValue = default)
        {
            if (!_configuration.TryGetValue(section, out var entries))
                _configuration.Add(section, entries = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase));

            if (!entries.ContainsKey(key))
                entries.Add(key, _config.Bind(section, key, defaultValue));
            return this;
        }

        public T Get<T>(string identifier)
        {
            var (section, key) = SplitIdentifier(identifier);
            return Get<T>(section, key);
        }

        public T Get<T>(string section, string key)
        {
            return _configuration.TryGetValue(section, out var entries) && entries.TryGetValue(key, out var entry) ?
                ((ConfigEntry<T>)entry).Value : default;
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
