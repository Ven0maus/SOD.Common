using BepInEx.Configuration;
using SOD.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

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
            var (section, key) = ConfigHelper.SplitIdentifier(identifier);
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
        /// Add a new non-generic entry to the configuration builder
        /// </summary>
        /// <param name="defaultValue"></param>
        /// <param name="identifier"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        public ConfigBuilder Add(object defaultValue, string identifier, string description)
        {
            var (section, key) = ConfigHelper.SplitIdentifier(identifier);
            return Add(defaultValue, section, key, description);
        }

        /// <summary>
        /// Add a new non-generic entry to the configuration builder
        /// </summary>
        /// <param name="defaultValue"></param>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        public ConfigBuilder Add(object defaultValue, string section, string key, string description)
        {
            if (!_configuration.TryGetValue(section, out var entries))
                _configuration.Add(section, entries = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase));

            if (!entries.ContainsKey(key))
                entries.Add(key, File.Bind(defaultValue, section, key, description));
            return this;
        }

        /// <summary>
        /// Set an existing entry to a new value in the configuration builder
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="identifier"></param>
        /// <param name="value"></param>
        public void Set<T>(string identifier, T value)
        {
            var (section, key) = ConfigHelper.SplitIdentifier(identifier);
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
        /// Set an existing non-generic entry to a new value in the configuration builder
        /// </summary>
        /// <param name="value"></param>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <exception cref="Exception"></exception>
        public void Set(object value, string section, string key)
        {
            if (ExistsInternal(section, key, out var entry))
                entry.BoxedValue = value;
            else
                throw new Exception($"No configuration entry exists for section \"{section}\" and key \"{key}\".");
        }

        /// <summary>
        /// Set an existing non-generic entry to a new value in the configuration builder
        /// </summary>
        /// <param name="value"></param>
        /// <param name="identifier"></param>
        public void Set(object value, string identifier)
        {
            var (section, key) = ConfigHelper.SplitIdentifier(identifier);
            Set(value, section, key);
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

        /// <summary>
        /// Check if a non-generic binding exists in the configuration builder
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Exists(out object entry, string section, string key)
        {
            var exists = ExistsInternal(section, key, out var config);
            entry = exists ? config.BoxedValue : default;
            return exists;
        }

        /// <summary>
        /// Check if a non-generic binding exists in the configuration builder
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="identifier"></param>
        /// <returns></returns>
        public bool Exists(out object entry, string identifier)
        {
            var exists = ExistsInternal(identifier, out var config);
            entry = exists ? config.BoxedValue : default;
            return exists;
        }

        internal bool ExistsInternal<T>(string identifier, out ConfigEntry<T> config)
        {
            var (section, key) = ConfigHelper.SplitIdentifier(identifier);
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
            var (section, key) = ConfigHelper.SplitIdentifier(identifier);
            return ExistsInternal(section, key, out config);
        }
        #endregion
    }
}