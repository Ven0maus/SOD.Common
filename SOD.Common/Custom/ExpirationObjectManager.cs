using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;

namespace SOD.Common.Custom
{
    /// <summary>
    /// A static helper class to track objects that expire after a set amount of in-game time and trigger an action on expiration.
    /// </summary>
    public static class ExpirationObjectManager
    {
        private static Dictionary<IExpirable, ExpirationObject> _expirableObjects;

        /// <summary>
        /// The collection of currently expiring objects.
        /// </summary>
        public static IReadOnlyList<IExpirable> ExpirableObjects => new List<IExpirable>((_expirableObjects ??= []).Keys);

        /// <summary>
        /// Returns the amount of expirable objects currently in the manager.
        /// </summary>
        public static int Count => _expirableObjects?.Count ?? 0;

        internal static void LoadState(string filePath)
        {
            var hash = Lib.SaveGame.GetUniqueString(filePath);
            var path = Lib.SaveGame.GetSavestoreDirectoryPath(Assembly.GetExecutingAssembly(), $"expiration_objects_{hash}.json");
            
            if (File.Exists(path))
            {
                try
                {
                    var json = File.ReadAllText(path);
                    _expirableObjects = JsonSerializer.Deserialize<Dictionary<IExpirable, ExpirationObject>>(json);

                    Plugin.Log.LogInfo("Loaded context for expiration objects.");
                }
                catch (Exception e)
                {
                    Plugin.Log.LogError($"Unable to load/parse expiration_objects_{hash}.json: " + e.Message);
                }
            }
        }

        internal static void SaveState(string filePath)
        {
            if (_expirableObjects != null && _expirableObjects.Count > 0)
            {
                var hash = Lib.SaveGame.GetUniqueString(filePath);
                try
                {
                    var json = JsonSerializer.Serialize(_expirableObjects);
                    var path = Lib.SaveGame.GetSavestoreDirectoryPath(Assembly.GetExecutingAssembly(), $"expiration_objects_{hash}.json");
                    File.WriteAllText(path, json);

                    Plugin.Log.LogInfo("Saved context for expiration objects.");
                }
                catch(Exception e)
                {
                    Plugin.Log.LogError($"Unable to save expiration_objects_{hash}.json: " + e.Message);
                }
            }
        }

        internal static void Update()
        {
            if (_expirableObjects == null || _expirableObjects.Count == 0) return;

            List<IExpirable> toBeRemoved = null;
            foreach (var expirationObject in _expirableObjects.Values)
            {
                if (expirationObject.ExpiresOn >= Lib.Time.CurrentDateTime)
                {
                    if (Plugin.InDebugMode)
                        Plugin.Log.LogInfo($"[DebugMode]: IExpirable \"{expirationObject.Expirable}\" has expired.");

                    expirationObject.Expirable.Expire();

                    toBeRemoved ??= [];
                    toBeRemoved.Add(expirationObject.Expirable);
                }
            }

            if (toBeRemoved != null)
            {
                foreach (var obj in toBeRemoved)
                    _expirableObjects.Remove(obj);

                if (_expirableObjects.Count == 0)
                    _expirableObjects = null;
            }
        }

        /// <summary>
        /// Registers the object to start tracking expiration.
        /// <br>Note: the expirable object must be serializable for save state to be compatible, so use public properties with accessible get and setters.</br>
        /// </summary>
        /// <param name="expirable"></param>
        public static void Register(IExpirable expirable, Helpers.Time.TimeData expiresOn)
        {
            if (expirable == null || (_expirableObjects != null && _expirableObjects.ContainsKey(expirable))) return;

            _expirableObjects ??= [];
            _expirableObjects.Add(expirable, new ExpirationObject(expirable, expiresOn));

            if (Plugin.InDebugMode)
                Plugin.Log.LogInfo($"[DebugMode]: Registered new IExpirable \"{expirable}\" expires on \"{expiresOn}\".");
        }

        class ExpirationObject(IExpirable expirable, Helpers.Time.TimeData expiresOn)
        {
            public Helpers.Time.TimeData ExpiresOn { get; } = expiresOn;
            public IExpirable Expirable { get; } = expirable;
        }
    }

    public interface IExpirable
    {
        void Expire();
    }
}
