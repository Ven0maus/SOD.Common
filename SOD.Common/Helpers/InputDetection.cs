using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Rewired;
using SOD.Common.Custom;
using SOD.Common.Extensions;
using UnityEngine;

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
        /// Get the Rewired controller binding currently bound to an InteractionKey's Rewired InputAction.
        /// </summary>
        /// <param name="interactionKey"></param>
        /// <returns></returns>
        public ActionElementMap GetBinding(InteractablePreset.InteractionKey interactionKey)
        {
            List<ActionElementMap> controllerMappings = KeyboardAndMouseMappings;
            try
            {
                return controllerMappings.First(x => x.actionId == GetRewiredAction(interactionKey).id);
            }
            catch (InvalidOperationException)
            {
                Plugin.Log.LogDebug($"Could not find action controller binding for interactionKey: {interactionKey}");
                return null;
            }
            catch (NullReferenceException)
            {
                Plugin.Log.LogDebug($"Null reference exception for interactionKey: {interactionKey}");
                return null;
            }
        }

        /// <summary>
        /// Raised when a button's state changes from up/released to
        /// down/pressed and vice versa. Is <b>not</b> raised for suppressed inputs,
        /// which are inputs that the vanilla game is currently being forced to
        /// ignore.
        /// </summary>
        public event EventHandler<InputDetectionEventArgs> OnButtonStateChanged;

        /// <summary>
        /// Raised when a button's state changes from up/released to
        /// down/pressed and vice versa. Is <b>only</b> raised for suppressed inputs,
        /// which are inputs that the vanilla game is currently being forced to
        /// ignore.
        /// </summary>
        public event EventHandler<SuppressedInputDetectionEventArgs> OnSuppressedButtonStateChanged;

        /// <summary>
        /// Raised when an axis's state changes in value. Is <b>not</b> raised
        /// for suppressed inputs, which are inputs that the vanilla game is
        /// currently being forced to ignore.
        /// </summary>
        public event EventHandler<AxisInputDetectionEventArgs> OnAxisStateChanged;

        /// <summary>
        /// Raised when an axis's state changes in value. Is <b>only</b> raised
        /// for suppressed inputs, which are inputs that the vanilla game is
        /// currently being forced to ignore.
        /// </summary>
        public event EventHandler<SuppressedAxisInputDetectionEventArgs> OnSuppressedAxisStateChanged;

        internal void ReportButtonStateChange(string actionName, InteractablePreset.InteractionKey interactionKey, bool isDown, List<string> suppressionEntryIds)
        {
            if (suppressionEntryIds.Count == 0)
                OnButtonStateChanged?.Invoke(this, new InputDetectionEventArgs(actionName, interactionKey, isDown));
            else
                OnSuppressedButtonStateChanged?.Invoke(this, new SuppressedInputDetectionEventArgs(actionName, interactionKey, isDown, suppressionEntryIds));
        }

        internal void ReportAxisStateChange(string actionName, InteractablePreset.InteractionKey interactionKey, float axisValue, List<string> suppressionEntryIds)
        {
            if (suppressionEntryIds.Count == 0)
                OnAxisStateChanged?.Invoke(this, new AxisInputDetectionEventArgs(actionName, interactionKey, axisValue));
            else
                OnSuppressedAxisStateChanged?.Invoke(this, new SuppressedAxisInputDetectionEventArgs(actionName, interactionKey, axisValue, suppressionEntryIds));
        }

        private List<ActionElementMap> keyboardAndMouseMappings;

        /// <summary>
        /// Contains all keyboard and mouse Rewired ActionElementMap mappings.
        /// </summary>
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
                        keyboardAndMouseMappings.AddRange(map.AllMaps.ToList());
                    }
                }
                return keyboardAndMouseMappings;
            }
        }

        internal Dictionary<string, InputSuppressionEntry> InputSuppressionDictionary { get; set; } = new();

        /// <summary>
        /// Suppress vanilla game responses to an input, with an optional duration. Adds an input suppression entry.
        /// <br>The input suppression entry will suppress vanilla game responses to the input while it or any entry tied to that input is present.</br>
        /// <br>At the end of the duration (if provided), the input suppression entry will be automatically removed.</br>
        /// </summary>
        /// <param name="id">The id associated with this entry.</param>
        /// <param name="keyCode">The KeyCode to be suppressed.</param>
        /// <param name="duration">The duration that the input suppression lasts. (does not progress while the game is paused).</param>
        /// <param name="overwrite">Whether to overwrite any existing entry with this id, applying the new duration.</param>
        public void SetInputSuppressed(string id, KeyCode keyCode, TimeSpan? duration = null, bool overwrite = false)
        {
            if (id == String.Empty)
                throw new ArgumentException("Id cannot be empty.", nameof(id));

            if (keyCode == UnityEngine.KeyCode.None)
                throw new ArgumentException("KeyCode cannot be none.", nameof(keyCode));

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
        /// <param name="interactionKey">The interaction (virtual action name) to be suppressed.</param>
        /// <param name="duration">The duration that the input suppression lasts. (does not progress while the game is paused).</param>
        /// <param name="overwrite">Whether to overwrite any existing entry with this id, applying the new duration.</param>
        public void SetInputSuppressed(string id, InteractablePreset.InteractionKey interactionKey, TimeSpan? duration = null, bool overwrite = false)
        {
            if (id == String.Empty)
                throw new ArgumentException("Id cannot be empty.", nameof(id));

            if (interactionKey == InteractablePreset.InteractionKey.none)
                throw new ArgumentException("InteractionKey cannot be none.", nameof(interactionKey));

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
        /// Stops and removes an input suppression entry, if present.
        /// </summary>
        /// <param name="id">The id associated with the entry.</param>
        public void RemoveInputSuppression(string id)
        {
            if (id == String.Empty)
                throw new ArgumentException("Key cannot be empty.", nameof(id));

            if (InputSuppressionDictionary.TryGetValue(id, out var entry))
            {
                entry.Stop();
                InputSuppressionDictionary.Remove(id);
            }
        }

        /// <summary>
        /// Stops and removes matching input suppression entries, if present.
        /// </summary>
        /// <param name="keyCode">The KeyCode to search for.</param>
        public void RemoveInputSuppression(KeyCode keyCode)
        {
            if (keyCode == UnityEngine.KeyCode.None)
                throw new ArgumentException("KeyCode cannot be none.", nameof(keyCode));

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
        /// Stops and removes matching input suppression entries, if present.
        /// </summary>
        /// <param name="interactionKey">The interaction to search for.</param>
        /// <param name="removeOverlappingEntries">Whether to remove entries which indirectly suppress the key code bound to the interaction, in addition to those which specifically target the interaction.</param>
        public void RemoveInputSuppression(InteractablePreset.InteractionKey interactionKey, bool removeOverlappingEntries)
        {
            if (interactionKey == InteractablePreset.InteractionKey.none)
                throw new ArgumentException("InteractionKey cannot be none.", nameof(interactionKey));
            if (!IsInputSuppressed(interactionKey, removeOverlappingEntries, out _, out _))
            {
                return;
            }

            List<string> keysToRemove = new();
            var binding = GetBinding(interactionKey);
            var boundKeyCode = GetApproximateKeyCode(binding);
            var elementIdentifierName = binding.elementIdentifierName;
            var elementIdentifierId = binding.elementIdentifierId;
            foreach (var (key, value) in InputSuppressionDictionary)
            {
                bool isOverlappingEntry = value.KeyCode == boundKeyCode || value.ElementIdentifierName == elementIdentifierName || value.ElementIdentifierId == elementIdentifierId;
                if (value.Id != key && (!removeOverlappingEntries || !isOverlappingEntry))
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
        /// Gets the KeyCode associated with a Rewired ActionElementMap binding. Required for certain Button-type bindings such as mouse buttons.
        /// </summary>
        /// <param name="binding"></param>
        /// <returns></returns>
        public KeyCode GetApproximateKeyCode(ActionElementMap binding)
        {
            if (binding.keyCode != KeyCode.None)
            {
                return binding.keyCode;
            }
            switch (binding.elementIdentifierName)
            {
                case "Left Mouse Button":
                    return KeyCode.Mouse0;
                case "Right Mouse Button":
                    return KeyCode.Mouse1;
                case "Mouse Button 3":
                    return KeyCode.Mouse2;
            }
            return KeyCode.None;
        }

        /// <summary>
        /// Determines if an input is being suppressed.
        /// </summary>
        /// <param name="keyCode">The KeyCode of the input.</param>
        /// <param name="suppressionEntryIds">All ids (dictionary entry keys) involved in suppressing the KeyCode.</param>
        /// <param name="maxTimeLeft">The maximum time that the input will be suppressed for, across all suppression entries.</param>
        /// <returns></returns>
        public bool IsInputSuppressed(KeyCode keyCode, out List<string> suppressionEntryIds, out TimeSpan? maxTimeLeft)
        {
            if (keyCode == KeyCode.None)
                throw new ArgumentException("IsInputSuppressed was passed a keycode value of KeyCode.None");

            suppressionEntryIds = new();
            maxTimeLeft = null;

            var isSuppressed = false;
            foreach (var (key, value) in InputSuppressionDictionary)
            {
                if (value.KeyCode != keyCode)
                {
                    continue;
                }
                isSuppressed = true;
                suppressionEntryIds.Add(value.Id);
                maxTimeLeft ??= TimeSpan.MinValue;
                if (value.TimeRemainingSec > 0f && value.TimeRemainingSec > maxTimeLeft.Value.TotalSeconds)
                    maxTimeLeft = TimeSpan.FromSeconds(value.TimeRemainingSec);
            }
            return isSuppressed;
        }

        /// <summary>
        /// Determines if an input is being suppressed.
        /// </summary>
        /// <param name="interactionKey">The interaction (virtual action name) associated with the input.</param>
        /// <param name="checkOverlappingEntries">Whether to account for indirect suppression of the key code bound to the interaction, in addition to targeted/specific suppression of the interaction.</param>
        /// <param name="suppressionEntryIds">All ids (dictionary entry keys) involved in suppressing the KeyCode.</param>
        /// <param name="maxTimeLeft">The maximum time that the input will be suppressed for, across all suppression entries.</param>
        /// <returns></returns>
        public bool IsInputSuppressed(InteractablePreset.InteractionKey interactionKey, bool checkOverlappingEntries, out List<string> suppressionEntryIds, out TimeSpan? maxTimeLeft)
        {
            suppressionEntryIds = new();
            maxTimeLeft = null;

            var binding = GetBinding(interactionKey);
            if (binding == null)
            {
                return false;
            }

            var boundKeyCode = GetApproximateKeyCode(binding);
            var elementIdentifierName = binding.elementIdentifierName;
            var elementIdentifierId = binding.elementIdentifierId;
            var isSuppressed = false;
            foreach (var (key, value) in InputSuppressionDictionary)
            {
                bool isOverlappingEntry = (boundKeyCode != KeyCode.None && value.KeyCode == boundKeyCode) || (value.ElementIdentifierName != string.Empty && value.ElementIdentifierName == elementIdentifierName && value.ElementIdentifierId == elementIdentifierId);
                if (value.InteractionKey != interactionKey && (!checkOverlappingEntries || !isOverlappingEntry))
                {
                    continue;
                }
                isSuppressed = true;
                suppressionEntryIds.Add(key);
                maxTimeLeft ??= TimeSpan.MinValue;
                if (value.TimeRemainingSec > 0f && value.TimeRemainingSec > maxTimeLeft.Value.TotalSeconds)
                    maxTimeLeft = TimeSpan.FromSeconds(value.TimeRemainingSec);
            }
            return isSuppressed;
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
                        InputSuppressionEntry entry;
                        if (data.ElementIdentifierName != string.Empty)
                        {
                            entry = new InputSuppressionEntry(data.Id, data.InteractionKey, data.Time == 0f ? null : TimeSpan.FromSeconds(data.Time));
                        }
                        else
                        {
                            entry = new InputSuppressionEntry(data.Id, data.KeyCode, data.Time == 0f ? null : TimeSpan.FromSeconds(data.Time));
                        }
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
            if (InputSuppressionDictionary.Count == 0)
            {
                if (File.Exists(storePath))
                    File.Delete(storePath);
                return;
            }

            var data = InputSuppressionDictionary
                .Select(a => new InputSuppressionEntry.JsonData
                {
                    Id = a.Key,
                    ElementIdentifierName = a.Value.ElementIdentifierName,
                    ElementIdentifierId = a.Value.ElementIdentifierId,
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
            foreach (var entry in InputSuppressionDictionary.Values)
                entry.Stop();
            InputSuppressionDictionary = new();
        }
    }

    public sealed class InputDetectionEventArgs : EventArgs
    {
        public string ActionName { get; }
        public InteractablePreset.InteractionKey Key { get; }
        public bool IsDown { get; }

        internal InputDetectionEventArgs(string actionName, InteractablePreset.InteractionKey key, bool isDown)
        {
            ActionName = actionName;
            Key = key;
            IsDown = isDown;
        }
    }

    public sealed class AxisInputDetectionEventArgs : EventArgs
    {
        public string ActionName { get; }
        public InteractablePreset.InteractionKey Key { get; }
        public float AxisValue { get; }

        internal AxisInputDetectionEventArgs(string actionName, InteractablePreset.InteractionKey key, float axisValue)
        {
            ActionName = actionName;
            Key = key;
            AxisValue = axisValue;
        }
    }

    public sealed class SuppressedInputDetectionEventArgs : EventArgs
    {
        public string ActionName { get; }
        public InteractablePreset.InteractionKey Key { get; }
        public bool IsDown { get; }
        public List<string> SuppressionEntryIds { get; }

        internal SuppressedInputDetectionEventArgs(string actionName, InteractablePreset.InteractionKey key, bool isDown, List<string> suppressionEntryIds)
        {
            ActionName = actionName;
            Key = key;
            IsDown = isDown;
            SuppressionEntryIds = suppressionEntryIds;
        }
    }

    public sealed class SuppressedAxisInputDetectionEventArgs : EventArgs
    {
        public string ActionName { get; }
        public InteractablePreset.InteractionKey Key { get; }
        public float AxisValue { get; }
        public List<string> SuppressionEntryIds { get; }

        internal SuppressedAxisInputDetectionEventArgs(string actionName, InteractablePreset.InteractionKey key, float axisValue, List<string> suppressionEntryIds)
        {
            ActionName = actionName;
            Key = key;
            AxisValue = axisValue;
            SuppressionEntryIds = suppressionEntryIds;
        }
    }
}
