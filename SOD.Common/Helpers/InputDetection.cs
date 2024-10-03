using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using BepInEx;
using Il2CppSystem.Linq;
using Il2CppSystem.Runtime.Remoting.Messaging;
using Rewired;
using Rewired.Demos;
using SOD.Common.Custom;
using SOD.Common.Extensions;

namespace SOD.Common.Helpers
{
    public sealed class InputDetection
    {
        private const int KEYBOARD_AND_MOUSE_CONTROLLER_ID = 0;
        private readonly List<string> CONTROLLER_MAP_CATEGORIES = ["Interaction", "Movement", "Menu", "CityEdit"];

        internal InputDetection() { }

        public Rewired.Player RewiredPlayer => InputController.Instance.player;

        /// <summary>
        /// Get the action name associated with an InteractionKey in a format that is useable with Rewired GetButton methods.
        /// </summary>
        /// <param name="interactionKey"></param>
        /// <returns></returns>
        public string GetRewiredActionName(InteractablePreset.InteractionKey interactionKey)
        {
            var gameMappedKey = Enum.GetName(typeof(InteractablePreset.InteractionKey), interactionKey);
            var capitalizedKey = $"{gameMappedKey.Substring(0, 1).ToUpper()}{gameMappedKey.Substring(1)}";
            return capitalizedKey;
        }

        /// <summary>
        /// Get the Rewired InputAction associated with an InteractionKey.
        /// </summary>
        /// <param name="interactionKey"></param>
        /// <returns></returns>
        public InputAction GetRewiredAction(InteractablePreset.InteractionKey interactionKey)
        {
            return ReInput.MappingHelper.Instance.GetAction(GetRewiredActionName(interactionKey));
        }

        /// <summary>
        /// Get the Unity KeyCode currently bound to an InteractionKey's Rewired InputAction.
        /// </summary>
        /// <param name="interactionKey"></param>
        /// <returns></returns>
        public UnityEngine.KeyCode GetBoundKeyCode(InteractablePreset.InteractionKey interactionKey)
        {
            List<ActionElementMap> controllerMappings = KeyboardAndMouseMappings;
            try
            {
                return controllerMappings.First(x => x.actionId == GetRewiredAction(interactionKey).id).keyCode;
            }
            catch (InvalidOperationException e)
            {
                return UnityEngine.KeyCode.None;
            }
        }

        /// <summary>
        /// Add a Rewired input event listener. Useful for advanced input events supported by Rewired, like double pressing, long presses, timed presses, etc. The listener will be invoked even when vanilla responses to input are being suppressed using SetInputSuppressed.
        /// </summary>
        /// <param name="interactionKey"></param>
        /// <param name="callback"></param>
        public void AddInputEventListener(InteractablePreset.InteractionKey interactionKey, Action<Rewired.InputActionEventData> callback)
        {
            RewiredPlayer.AddInputEventDelegate(callback, UpdateLoopType.Update, GetRewiredAction(interactionKey).id);
        }

        /// <summary>
        /// Remove a Rewired input event listener.
        /// </summary>
        /// <param name="interactionKey"></param>
        /// <param name="callback"></param>
        public void RemoveInputEventListener(InteractablePreset.InteractionKey interactionKey, Action<Rewired.InputActionEventData> callback)
        {
            ReInput.PlayerHelper.Instance.GetPlayer(0).RemoveInputEventDelegate(callback, UpdateLoopType.Update, GetRewiredAction(interactionKey).id);
        }

        /// <summary>
        /// Raised when a button's state changes from up/released to down/pressed and vice versa.
        /// </summary>
        public event EventHandler<InputDetectionEventArgs> OnButtonStateChanged;

        internal void ReportButtonStateChange(string actionName, InteractablePreset.InteractionKey key, bool isDown, bool isSuppressed)
        {
            OnButtonStateChanged?.Invoke(this, new InputDetectionEventArgs(actionName, key, isDown, isSuppressed));
        }

        private List<ActionElementMap> keyboardAndMouseMappings;
        public List<ActionElementMap> KeyboardAndMouseMappings
        {
            get
            {
                if (keyboardAndMouseMappings != null)
                {
                    return keyboardAndMouseMappings;
                }
                keyboardAndMouseMappings = new List<ActionElementMap>();
                var controllers = new List<Rewired.Controller>();
                Rewired.Controller keyboardController = RewiredPlayer.controllers.GetController(ControllerType.Keyboard, KEYBOARD_AND_MOUSE_CONTROLLER_ID);
                controllers.Add(keyboardController);
                Rewired.Controller mouseController = RewiredPlayer.controllers.GetController(ControllerType.Mouse, KEYBOARD_AND_MOUSE_CONTROLLER_ID);
                controllers.Add(mouseController);
                foreach (var controller in controllers)
                {
                    foreach (string category in CONTROLLER_MAP_CATEGORIES)
                    {
                        ControllerMap map = RewiredPlayer.controllers.maps.GetMap(controller.type, controller.id, category, "Default");
                        KeyboardAndMouseMappings.AddRange(map.AllMaps.ToList());
                    }
                }
                return keyboardAndMouseMappings;
            }
        }

        // Used to track which inputs are being suppressed such that vanilla game responses to suppressed events are bypassed.
        internal Dictionary<string, InputSuppressionEntry> InputSuppressionDictionary { get; set; }

        /// <summary>
        /// Suppress vanilla game responses to an input, with an optional duration. Adds an input suppression entry.
        /// <br>The input suppression entry will suppress vanilla game responses to the input while it or any entry tied to that input is present.</br>
        /// <br>At the end of the duration (if provided), the input suppression entry will be automatically removed.</br>
        /// </summary>
        /// <param name="id">The id associated with this entry.</param>
        /// <param name="keyCode">The KeyCode to be suppressed.</param>
        /// <param name="duration">The duration that the input suppression lasts. (does not progress while the game is paused).</param>
        /// <param name="overwrite">Whether to overwrite any existing entry with this id, applying the new duration.</param>
        public void SetInputSuppressed(string id, UnityEngine.KeyCode keyCode, TimeSpan? duration = null, bool overwrite = false)
        {
            if (id == String.Empty)
                throw new ArgumentException("Id cannot be empty.", nameof(id));

            if (keyCode == UnityEngine.KeyCode.None)
                throw new ArgumentException("KeyCode cannot be none.", nameof(keyCode));

            InputSuppressionDictionary ??= new();

            if (InputSuppressionDictionary.TryGetValue(id, out var entry))
            {
                if (overwrite)
                {
                    entry.Stop();
                    InputSuppressionDictionary.Remove(id);
                }
                else
                {
                    return;
                }
            }

            // Add modifier to dictionary to track
            entry = new InputSuppressionEntry(id, keyCode, duration);
            InputSuppressionDictionary.Add(id, entry);

            // Start timer
            entry.Start();
        }

        /// <summary>
        /// Suppress vanilla game responses to an input, with an optional duration. Adds an input suppression entry.
        /// <br>The input suppression entry will suppress vanilla game responses to the input while it or any entry tied to that input is present.</br>
        /// <br>At the end of the duration (if provided), the input suppression entry will be automatically removed.</br>
        /// </summary>
        /// <param name="id">The id associated with this entry.</param>
        /// <param name="interactionKey">The interaction to be suppressed.</param>
        /// <param name="duration">The duration that the input suppression lasts. (does not progress while the game is paused).</param>
        /// <param name="overwrite">Whether to overwrite any existing entry with this id, applying the new duration.</param>
        public void SetInputSuppressed(string id, InteractablePreset.InteractionKey interactionKey, TimeSpan? duration = null, bool overwrite = false)
        {
            if (id == String.Empty)
                throw new ArgumentException("Id cannot be empty.", nameof(id));

            if (interactionKey == InteractablePreset.InteractionKey.none)
                throw new ArgumentException("InteractionKey cannot be none.", nameof(interactionKey));

            InputSuppressionDictionary ??= new();

            if (InputSuppressionDictionary.TryGetValue(id, out var entry))
            {
                if (overwrite)
                {
                    entry.Stop();
                    InputSuppressionDictionary.Remove(id);
                }
                else
                {
                    return;
                }
            }

            // Add modifier to dictionary to track
            entry = new InputSuppressionEntry(id, interactionKey, duration);
            InputSuppressionDictionary.Add(id, entry);

            // Start timer
            entry.Start();
        }

        /// <summary>
        /// Removes an input suppression entry, if present.
        /// </summary>
        /// <param name="id">The id associated with the entry.</param>
        public void RemoveInputSuppression(string id)
        {
            if (id == String.Empty)
                throw new ArgumentException("Key cannot be empty.", nameof(id));

            if (InputSuppressionDictionary != null && InputSuppressionDictionary.TryGetValue(id, out var entry))
            {
                entry.Stop();
                InputSuppressionDictionary.Remove(id);
            }
        }

        /// <summary>
        /// Removes matching input suppression entries, if present.
        /// </summary>
        /// <param name="keyCode">The KeyCode to search for.</param>
        public void RemoveInputSuppression(UnityEngine.KeyCode keyCode)
        {
            if (keyCode == UnityEngine.KeyCode.None)
                throw new ArgumentException("KeyCode cannot be none.", nameof(keyCode));

            if (InputSuppressionDictionary == null)
            {
                return;
            }

            List<string> keysToRemove = new();
            foreach (var (key, value) in InputSuppressionDictionary)
            {
                if (value.KeyCode != keyCode)
                {
                    continue;
                }
                value.Stop();
                keysToRemove.Add(key);
            }
            foreach (var i in keysToRemove)
            {
                InputSuppressionDictionary.Remove(i);
            }
        }

        /// <summary>
        /// Removes matching input suppression entries, if present.
        /// </summary>
        /// <param name="interactionKey">The interaction to search for.</param>
        /// <param name="includeBoundKeyCodeEntries">Whether to remove entries which indirectly suppress the key code bound to the interaction, in addition to those which specifically target the interaction.</param>
        public void RemoveInputSuppression(InteractablePreset.InteractionKey interactionKey, bool includeBoundKeyCodeEntries)
        {
            if (interactionKey == InteractablePreset.InteractionKey.none)
                throw new ArgumentException("InteractionKey cannot be none.", nameof(interactionKey));

            if (InputSuppressionDictionary == null)
            {
                return;
            }

            List<string> keysToRemove = new();
            var boundKeyCode = GetBoundKeyCode(interactionKey);
            foreach (var (key, value) in InputSuppressionDictionary)
            {
                if (value.InteractionKey != interactionKey && (!includeBoundKeyCodeEntries || value.KeyCode != boundKeyCode))
                {
                    continue;
                }
                value.Stop();
                keysToRemove.Add(key);
            }
            foreach (var i in keysToRemove)
            {
                InputSuppressionDictionary.Remove(i);
            }
        }

        /// <summary>
        /// Determines if an input is being suppressed.
        /// </summary>
        /// <param name="keyCode">The KeyCode of the input.</param>
        /// <param name="timeLeft"></param>
        /// <returns></returns>
        public bool IsInputSuppressed(UnityEngine.KeyCode keyCode, out TimeSpan? timeLeft)
        {
            timeLeft = null;
            if (InputSuppressionDictionary == null)
            {
                return false;
            }

            foreach (var (key, value) in InputSuppressionDictionary)
            {
                if (value.KeyCode != keyCode)
                // if (value.KeyCode != keyCode && (!includeBoundKeyCodeEntries || value.KeyCode != boundKeyCode))
                {
                    continue;
                }
                if (value.TimeRemainingSec > 0f)
                    timeLeft = TimeSpan.FromSeconds(value.TimeRemainingSec);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Determines if an input is being suppressed.
        /// </summary>
        /// <param name="interactionKey">The interaction associated with the input.</param>
        /// <param name="includeBoundKeyCodeEntries">Whether to account for indirect suppression of the key code bound to the interaction, in addition to targeted/specific suppression of the interaction.</param>
        /// <param name="timeLeft"></param>
        /// <returns></returns>
        public bool IsInputSuppressed(InteractablePreset.InteractionKey interactionKey, bool includeBoundKeyCodeEntries, out TimeSpan? timeLeft)
        {
            timeLeft = null;
            if (InputSuppressionDictionary == null)
            {
                return false;
            }

            var boundKeyCode = GetBoundKeyCode(interactionKey);
            foreach (var (key, value) in InputSuppressionDictionary)
            {
                if (value.InteractionKey != interactionKey && (!includeBoundKeyCodeEntries || value.KeyCode != boundKeyCode))
                {
                    continue;
                }
                if (value.TimeRemainingSec > 0f)
                    timeLeft = TimeSpan.FromSeconds(value.TimeRemainingSec);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Called when a savegame is loaded.
        /// </summary>
        /// <param name="path"></param>
        internal void Load(string path)
        {
            var hash = Lib.SaveGame.GetUniqueString(path);
            var storePath = Lib.SaveGame.GetSavestoreDirectoryPath(Assembly.GetExecutingAssembly(), $"inputsuppression_{hash}.json");

            if (File.Exists(storePath))
            {
                var json = File.ReadAllText(storePath);
                var jsonData = JsonSerializer.Deserialize<List<InputSuppressionEntry.JsonData>>(json);

                if (jsonData.Count > 0)
                {
                    InputSuppressionDictionary = new();
                    foreach (var data in jsonData)
                    {
                        var entry = new InputSuppressionEntry(data.Id, data.KeyCode, data.Time == 0f ? null : TimeSpan.FromSeconds(data.Time));
                        InputSuppressionDictionary[data.Id] = entry;
                        entry.Start();
                    }
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
            var storePath = Lib.SaveGame.GetSavestoreDirectoryPath(Assembly.GetExecutingAssembly(), $"inputsuppression_{hash}.json");

            // Clean-up
            if (InputSuppressionDictionary == null || InputSuppressionDictionary.Count == 0)
            {
                if (File.Exists(storePath))
                    File.Delete(storePath);
                return;
            }

            var data = InputSuppressionDictionary
                .Select(a => new InputSuppressionEntry.JsonData
                {
                    Id = a.Key,
                    KeyCode = a.Value.KeyCode,
                    InteractionKey = a.Value.InteractionKey,
                    Time = a.Value.TimeRemainingSec
                })
                .ToList();
            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = false });
            File.WriteAllText(storePath, json);
        }

        /// <summary>
        /// Method is used to reset the player status data tracked
        /// </summary>
        internal void ResetSuppressionTracking()
        {
            // Clear out the modifiers and the dictionary for a new game
            if (InputSuppressionDictionary != null)
            {
                foreach (var entry in InputSuppressionDictionary.Values)
                    entry.Stop();
                InputSuppressionDictionary = null;
            }
        }
    }

    public sealed class InputDetectionEventArgs : EventArgs
    {
        public string ActionName { get; }
        public InteractablePreset.InteractionKey Key { get; }
        public bool IsDown { get; }
        public bool IsSuppressed { get; }

        internal InputDetectionEventArgs(string actionName, InteractablePreset.InteractionKey key, bool isDown, bool isSuppressed)
        {
            ActionName = actionName;
            Key = key;
            IsDown = isDown;
            IsSuppressed = isSuppressed;
        }
    }
}
