using BepInEx.Configuration;
using System;
using System.Collections.Generic;

namespace SOD.Common.BepInEx.Configuration
{
    public sealed class ConfigBuilder
    {
        private readonly Dictionary<string, Dictionary<string, object>> _configuration = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// The base configuration file, can be used to modify certain settings.
        /// </summary>
        public ConfigFile File { get; }

        internal ConfigBuilder(ConfigFile config)
        {
            File = config;
        }

        /// <summary>
        /// Add a new entry to the configuration builder
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="identifier"></param>
        /// <param name="defaultValue"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        public ConfigBuilder Add<T>(string identifier, string description, T defaultValue = default)
        {
            var (section, key) = SplitIdentifier(identifier);
            return Add(section, key, description, defaultValue);
        }

        /// <summary>
        /// Add a new entry to the configuration builder
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        public ConfigBuilder Add<T>(string section, string key, string description, T defaultValue = default)
        {
            if (!_configuration.TryGetValue(section, out var entries))
                _configuration.Add(section, entries = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase));

            if (!entries.ContainsKey(key))
                entries.Add(key, File.Bind(section, key, defaultValue, description));
            return this;
        }

        /// <summary>
        /// Get an existing entry out of the configuration builder
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="identifier"></param>
        /// <returns></returns>
        public T Get<T>(string identifier)
        {
            var (section, key) = SplitIdentifier(identifier);
            return Get<T>(section, key);
        }

        /// <summary>
        /// Get an existing entry out of the configuration builder
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Set an existing entry to a new value in the configuration builder
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="identifier"></param>
        /// <param name="value"></param>
        public void Set<T>(string identifier, T value)
        {
            var (section, key) = SplitIdentifier(identifier);
            Set(section, key, value);
        }

        /// <summary>
        /// Set an existing entry to a new value in the configuration builder
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <exception cref="Exception"></exception>
        public void Set<T>(string section, string key, T value)
        {
            if (ExistsInternal<T>(section, key, out var entry))
                entry.BoxedValue = value;
            else
                throw new Exception($"No configuration entry exists for section \"{section}\" and key \"{key}\".");
        }

        /// <summary>
        /// Check if a binding exists in the configuration builder
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="identifier"></param>
        /// <param name="entry"></param>
        /// <returns></returns>
        public bool Exists<T>(string identifier, out T entry)
        {
            var exists = ExistsInternal<T>(identifier, out var config);
            entry = exists ? config.Value : default;
            return exists;
        }

        /// <summary>
        /// Check if a binding exists in the configuration builder
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="entry"></param>
        /// <returns></returns>
        public bool Exists<T>(string section, string key, out T entry)
        {
            var exists = ExistsInternal<T>(section, key, out var config);
            entry = exists ? config.Value : default;
            return exists;
        }

        internal bool ExistsInternal<T>(string identifier, out ConfigEntry<T> config)
        {
            var (section, key) = SplitIdentifier(identifier);
            return ExistsInternal(section, key, out config);
        }

        internal bool ExistsInternal<T>(string section, string key, out ConfigEntry<T> config)
        {
            config = null;
            if (_configuration.TryGetValue(section, out var entries) && entries.TryGetValue(key, out var entry))
            {
                config = (ConfigEntry<T>)entry;
                return true;
            }
            return false;
        }

        #region Proxy
        internal bool ExistsInternal(string section, string key, out ConfigEntryBase config)
        {
            config = null;
            if (_configuration.TryGetValue(section, out var entries) && entries.TryGetValue(key, out var entry))
            {
                config = (ConfigEntryBase)entry;
                return true;
            }
            return false;
        }

        internal bool ExistsInternal(string identifier, out ConfigEntryBase config)
        {
            var (section, key) = SplitIdentifier(identifier);
            return ExistsInternal(section, key, out config);
        }
        #endregion

        private static (string section, string key) SplitIdentifier(string identifier)
        {
            var parts = identifier.Split('.');
            if (parts.Length != 2)
                throw new Exception($"Invalid configuration identifier \"{identifier}\" provided.");
            return (parts[0], parts[1]);
        }
    }
}