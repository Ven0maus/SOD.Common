using SOD.Common.Custom;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace SOD.Common.Helpers
{
    public sealed class PlayerStatus
    {
        internal PlayerStatus() { }

        /// <summary>
        /// Raised right before illegal action status will be set (either enabled or disabled).
        /// </summary>
        public event EventHandler<IllegalActionStatusArgs> OnBeforeSetIllegalStatus;
        /// <summary>
        /// Raised right after illegal action status is successfully set (either enabled or disabled).
        /// </summary>
        public event EventHandler<IllegalActionStatusArgs> OnAfterSetIllegalStatus;

        /// <summary>
        /// Dictionary used to track illegal status for the player entity.
        /// </summary>
        internal Dictionary<string, IllegalStatusModifier> IllegalStatusModifierDictionary { get; private set; }

        /// <summary>
        /// Sets an illegal status modifier with an optional duration.
        /// <br>The modifier will cause the Player to have an Illegal Action status while it or any other modifier is present.</br>
        /// <br>At the end of the duration (if provided), the modifier will be automatically removed.</br>
        /// </summary>
        /// <param name="key">The key identifying the modifier. Used to overwrite, remove, or toggle the modifier later.</param>
        /// <param name="duration">The duration that the modifier lasts. (does not progress while the game is paused).</param>
        /// <param name="overwrite">Whether to overwrite any existing modifier with this key, applying the new duration.</param>
        public void SetIllegalStatusModifier(string key, TimeSpan? duration = null, bool overwrite = false)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Key cannot be null or empty.", nameof(key));

            IllegalStatusModifierDictionary ??= new();

            if (IllegalStatusModifierDictionary.TryGetValue(key, out var modifier))
            {
                if (overwrite)
                {
                    modifier.Stop();
                    IllegalStatusModifierDictionary.Remove(key);
                }
                else
                {
                    return;
                }
            }

            // Add modifier to dictionary to track
            modifier = new IllegalStatusModifier(key, duration);
            IllegalStatusModifierDictionary.Add(key, modifier);

            // Start timer
            modifier.Start();

            // Update status
            UpdatePlayerIllegalStatus();
        }

        /// <summary>
        /// Removes an illegal status modifier, if present.
        /// </summary>
        /// <param name="key">The key identifying the modifier.</param>
        public void RemoveIllegalStatusModifier(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Key cannot be null or empty.", nameof(key));

            if (IllegalStatusModifierDictionary != null && IllegalStatusModifierDictionary.TryGetValue(key, out var modifier))
            {
                modifier.Stop();
                IllegalStatusModifierDictionary.Remove(key);

                // Update status
                UpdatePlayerIllegalStatus();
            }
        }

        /// <summary>
        /// Determines if a modifier exists with the given key.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="timeLeft"></param>
        /// <returns></returns>
        public bool ModifierExists(string key, out TimeSpan? timeLeft)
        {
            timeLeft = null;
            if (IllegalStatusModifierDictionary != null && IllegalStatusModifierDictionary.TryGetValue(key, out var modifier))
            {
                if (modifier.TimeRemainingSec > 0f)
                    timeLeft = TimeSpan.FromSeconds(modifier.TimeRemainingSec);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Update the player's illegal status based on the information we are tracking.
        /// </summary>
        internal void UpdatePlayerIllegalStatus()
        {
            if (IllegalStatusModifierDictionary != null && IllegalStatusModifierDictionary.Any())
                AdjustIllegalStatus(Player.Instance.illegalStatus, true);
            else
                AdjustIllegalStatus(Player.Instance.illegalStatus, false);
        }

        /// <summary>
        /// Called when a savegame is loaded.
        /// </summary>
        /// <param name="path"></param>
        internal void Load(string path)
        {
            var hash = Lib.SaveGame.GetUniqueString(path);
            var storePath = Lib.SaveGame.GetSavestoreDirectoryPath(Assembly.GetExecutingAssembly(), $"playerstatus_{hash}");

            if (File.Exists(storePath))
            {
                var json = File.ReadAllText(storePath);
                var jsonData = JsonSerializer.Deserialize<List<IllegalStatusModifier.JsonData>>(json);

                if (jsonData.Count > 0)
                {
                    IllegalStatusModifierDictionary = new Dictionary<string, IllegalStatusModifier>();
                    foreach (var data in jsonData)
                    {
                        var modifier = new IllegalStatusModifier(data.Key, data.Time == 0f ? null : TimeSpan.FromSeconds(data.Time));
                        IllegalStatusModifierDictionary[data.Key] = modifier;
                        modifier.Start();
                    }

                    // Update player status
                    UpdatePlayerIllegalStatus();
                }
            }
        }

        /// <summary>
        /// Called when a savegame is saved.
        /// </summary>
        /// <param name="path"></param>
        internal void Save(string path)
        {
            var hash = Lib.SaveGame.GetUniqueString(path);
            var storePath = Lib.SaveGame.GetSavestoreDirectoryPath(Assembly.GetExecutingAssembly(), $"playerstatus_{hash}");

            // Clean-up
            if (IllegalStatusModifierDictionary == null || IllegalStatusModifierDictionary.Count == 0)
            {
                if (File.Exists(storePath))
                    File.Delete(storePath);
                return;
            }

            var data = IllegalStatusModifierDictionary
                .Select(a => new IllegalStatusModifier.JsonData 
                { 
                    Key = a.Key, 
                    Time = a.Value.TimeRemainingSec 
                })
                .ToList();
            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = false });
            File.WriteAllText(storePath, json);
        }

        /// <summary>
        /// Method is used to reset the player status data tracked
        /// </summary>
        internal void ResetStatusTracking()
        {
            // Clear out the modifiers and the dictionary for a new game
            if (IllegalStatusModifierDictionary != null)
            {
                foreach (var modifier in IllegalStatusModifierDictionary.Values)
                    modifier.Stop();
                IllegalStatusModifierDictionary = null;
            }
        }

        private static void AdjustIllegalStatus(bool previousStatus, bool newStatus)
        {
            if (previousStatus != newStatus)
            {
                var args = new IllegalActionStatusArgs(previousStatus, newStatus);

                // Raise before event
                Lib.PlayerStatus.OnSetIllegalActionStatus(args, false);

                // Adjust illegal status
                Player.Instance.illegalActionActive = newStatus;
                Player.Instance.illegalActionTimer = newStatus ? float.MaxValue : 0f;
                Player.Instance.UpdateIllegalStatus();

                // Raise after event
                Lib.PlayerStatus.OnSetIllegalActionStatus(args, newStatus);
            }
        }

        public sealed class IllegalActionStatusArgs : EventArgs
        {
            public bool PreviousIllegalStatus { get; }
            public bool NewIllegalStatus { get; }

            internal IllegalActionStatusArgs(bool previous, bool @new)
            {
                PreviousIllegalStatus = previous;
                NewIllegalStatus = @new;
            }
        }

        internal void OnSetIllegalActionStatus(IllegalActionStatusArgs eventArgs, bool after)
        {
            if (after)
                OnAfterSetIllegalStatus?.Invoke(this, eventArgs);
            else
                OnBeforeSetIllegalStatus?.Invoke(this, eventArgs);
        }
    }
}
