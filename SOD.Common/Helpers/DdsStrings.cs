using System;

namespace SOD.Common.Helpers
{
    public sealed class DdsStrings
    {
        internal DdsStrings() { }

        /// <summary>
        /// Indexer to retrieve a dds entry.
        /// </summary>
        /// <param name="dictionary"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public string this[string dictionary, string key]
        {
            get 
            {
                if (Strings.Instance == null || Strings.stringTable == null) 
                    return string.Empty;

                if (!Strings.textFilesLoaded)
                {
                    Strings.Instance.LoadTextFiles();
                }

                if (Strings.stringTable.TryGetValue(dictionary.ToLower(), out var table) &&
                    table.TryGetValue(key.ToLower(), out var ddsString))
                    return ddsString.displayStr;
                return string.Empty;
            }
            set 
            {
                if (Strings.Instance == null || Strings.stringTable == null)
                    throw new Exception("DDS Strings are not yet initialized at this point.");

                if (!Strings.textFilesLoaded)
                {
                    Strings.Instance.LoadTextFiles();
                }

                var dictionaryLower = dictionary.ToLower();
                var keyLower = key.ToLower();
                if (Strings.stringTable.TryGetValue(dictionaryLower, out var table))
                {
                    if (table.TryGetValue(keyLower, out var ddsString))
                    {
                        if (value == null)
                        {
                            table.Remove(keyLower);
                            if (table.Count == 0)
                                Strings.stringTable.Remove(dictionaryLower);
                            return;
                        }    
                        ddsString.displayStr = value;
                    }
                    else
                    {
                        if (value == null)
                        {
                            if (table.Count == 0)
                                Strings.stringTable.Remove(dictionaryLower);
                            return;
                        }
                        table[keyLower] = new Strings.DisplayString() { displayStr = value };
                    }
                }
                else
                {
                    if (value == null) return;
                    var dict = new Il2CppSystem.Collections.Generic.Dictionary<string, Strings.DisplayString>();
                    dict.Add(keyLower, new Strings.DisplayString { displayStr = value });
                    Strings.stringTable.Add(dictionaryLower, dict);
                }

                Plugin.Log.LogInfo($"Set: \"{dictionaryLower}\" | \"{keyLower}\" |-> \"{value}\"");
            }
        }

        /// <summary>
        /// Add's or updates a value in the dds entries.
        /// </summary>
        /// <param name="dictionary"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void AddOrUpdate(string dictionary, string key, string value)
        {
            this[dictionary, key] = value;
        }

        /// <summary>
        /// Removes an existing dds entry.
        /// </summary>
        /// <param name="dictionary"></param>
        /// <param name="key"></param>
        public void Remove(string dictionary, string key)
        {
            this[dictionary, key] = null;
        }

        /// <summary>
        /// Get's an existing dds entry.
        /// </summary>
        /// <param name="dictionary"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public string Get(string dictionary, string key)
        {
            return this[dictionary, key];
        }

        /// <summary>
        /// Add's or updates multiple keys at once in the dds entries.
        /// </summary>
        /// <param name="dictionary"></param>
        /// <param name="entries"></param>
        public void AddOrUpdateEntries(string dictionary, params (string key, string value)[] entries)
        {
            if (entries == null) return;
            foreach (var (key, value) in entries)
                this[dictionary, key] = value;
        }

        /// <summary>
        /// Removes multiple keys from the dds entries at once.
        /// </summary>
        /// <param name="dictionary"></param>
        /// <param name="entries"></param>
        public void RemoveEntries(string dictionary, params string[] entries)
        {
            if (entries == null) return;
            foreach (var entry in entries)
                this[dictionary, entry] = null;
        }
    }
}
