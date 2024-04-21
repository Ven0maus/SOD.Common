using System;
using System.Collections;
using System.Collections.Generic;
using SOD.Common.Custom;
using SOD.Common.Extensions;
using UnityEngine;
using UniverseLib;

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

        internal void OnSetIllegalActionStatus(IllegalActionStatusArgs eventArgs, bool after)
        {
            if (after)
                OnAfterSetIllegalStatus?.Invoke(this, eventArgs);
            else
                OnBeforeSetIllegalStatus?.Invoke(this, eventArgs);
        }

        internal Dictionary<string, IllegalStatusModifier> IllegalStatusModifierDictionary = new Dictionary<string, IllegalStatusModifier>();

        internal Coroutine UpdateIllegalStatusModifiersCoroutine { get; set; }

        /// <summary>
        /// Sets an illegal status modifier for the specified duration, or
        /// indefinitely if the duration is 0.0f or omitted. The modifier will
        /// cause the Player to have an Illegal Action status while it or any
        /// other modifier is present. At the end of the duration (if
        /// provided), the modifier will be automatically removed.
        /// </summary>
        /// <param name="key">The key identifying the modifier. Used to overwrite, remove, or toggle the modifier later.</param>
        /// <param name="force">Whether to overwrite any existing modifier with this key, applying the new duration.</param>
        /// <param name="duration">The duration that the modifier lasts, in seconds (does not progress while the game is paused).</param>
        public void SetIllegalStatusModifier(string key, bool force = false, float duration = 0.0f)
        {
            if (key == "")
            {
                throw new InvalidOperationException("Empty keys are prohibited.");
            }
            if (force && IllegalStatusModifierDictionary.ContainsKey(key))
            {
                // Remove entry with the same key, if it exists. Do not stop
                // the coroutine if it is started.
                IllegalStatusModifierDictionary.Remove(key);
            }
            else if (!force)
            {
                // Modifier with this key is already present
                return;
            }
            IllegalStatusModifierDictionary.Add(key, new IllegalStatusModifier(key, duration, duration == 0.0f ? false : true));
            if (UpdateIllegalStatusModifiersCoroutine == null)
            {
                UpdateIllegalStatusModifiersCoroutine = RuntimeHelper.StartCoroutine(UpdateIllegalStatusModifiers());
            }
        }

        /// <summary>
        /// Removes an illegal status modifier, if present.
        /// </summary>
        /// <param name="key">The key identifying the modifier.</param>
        public void RemoveIllegalStatusModifier(string key)
        {
            if (key == "")
            {
                // No need to throw
                return;
            }
            if (IllegalStatusModifierDictionary.ContainsKey(key))
            {
                IllegalStatusModifierDictionary.Remove(key);
            }
            if (IllegalStatusModifierDictionary.Count > 0)
            {
                return;
            }
            if (UpdateIllegalStatusModifiersCoroutine != null)
            {
                RuntimeHelper.StopCoroutine(UpdateIllegalStatusModifiersCoroutine);
                UpdateIllegalStatusModifiersCoroutine = null;
            }
        }

        /// <summary>
        /// Toggles an illegal status modifier by adding it if the key is not present, or removing it if the key is present. If added, the modifier will not have a duration associated with it (it will be active indefinitely, until toggled or removed).
        /// </summary>
        /// <param name="key">The key identifying the modifier.</param>
        public void ToggleIllegalStatusModifier(string key)
        {
            if (IllegalStatusModifierDictionary.ContainsKey(key))
            {
                RemoveIllegalStatusModifier(key);
                return;
            }
            SetIllegalStatusModifier(key);
        }

        internal IEnumerator UpdateIllegalStatusModifiers()
        {
            var setAsIllegal = false;
            var modifierKeysToRemove = new List<string>();
            var deltaTime = 0.0f;
            while (true)
            {
                yield return new WaitForEndOfFrame();

                if (Lib.Time.IsGamePaused)
                {
                    continue;
                }

                setAsIllegal = false;
                deltaTime = UnityEngine.Time.deltaTime;

                foreach (var key in IllegalStatusModifierDictionary.Keys)
                {
                    var entry = IllegalStatusModifierDictionary[key];
                    if (!entry.UseTime)
                    {
                        setAsIllegal = true;
                        continue;
                    }
                    entry.TimeRemainingSec -= deltaTime;
                    if (entry.TimeRemainingSec > 0.0f)
                    {
                        setAsIllegal = true;
                        continue;
                    }
                    // Expired timer, so ignore it and queue the modifier to be
                    // removed
                    modifierKeysToRemove.Add(entry.Key);
                }

                modifierKeysToRemove.ForEach(key => IllegalStatusModifierDictionary.Remove(key));
                modifierKeysToRemove.Clear();

                if (Player.Instance.illegalStatus == setAsIllegal)
                {
                    // Avoid constant changing back and forth
                    Player.Instance.illegalActionTimer = deltaTime * 2.0f;
                    continue;
                }

                // We want our illegal status to fall off as soon as the
                // dictionary is empty, but to last until at least the next
                // frame.
                // Use deltaTime * 2.0f to prevent issues where the timer
                // immediately expires during the UpdateIllegalStatus call
                var args = new IllegalActionStatusArgs(setAsIllegal);
                Lib.PlayerStatus.OnSetIllegalActionStatus(args, false);
                Player.Instance.illegalActionActive = setAsIllegal;
                Player.Instance.illegalActionTimer = deltaTime * 2.0f;
                Player.Instance.UpdateIllegalStatus();
                Lib.PlayerStatus.OnSetIllegalActionStatus(args, true);
            }
        }

        public sealed class IllegalActionStatusArgs : EventArgs
        {
            public bool SetAsIllegal { get; }

            internal IllegalActionStatusArgs(bool setAsIllegal)
            {
                SetAsIllegal = setAsIllegal;
            }
        }
    }

}
