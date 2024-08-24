using HarmonyLib;
using SOD.Common;
using SOD.Common.Helpers.SyncDiskObjects;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SOD.LifeAndLiving.Content.SyncDisks
{
    /// <summary>
    /// Add's a sync disk that can recognize the chatter of npcs which are selected on your current selected case board as evidence.
    /// </summary>
    internal class RecognizeNpcs
    {
        private readonly int _effectId;

        /// <summary>
        /// Determines if the effect to recognize npcs is enabled.
        /// </summary>
        internal bool Enabled { get; private set; }

        public RecognizeNpcs() 
        {
            Lib.SyncDisks.OnAfterSyncDiskInstalled += OnSyncDiskInstall;
            Lib.SyncDisks.OnAfterSyncDiskUninstalled += OnSyncDiskUninstall;

            // Create and register the sync disk
            _ = Lib.SyncDisks.Builder("Echolocation", Plugin.PLUGIN_GUID)
                .SetPrice(2500)
                .SetManufacturer(SyncDiskPreset.Manufacturer.BlackMarket)
                .SetRarity(SyncDiskPreset.Rarity.veryRare)
                .AddEffect("Echolocation", "Highlights civilian chatter for your selected case if their voice is identified and their profile is on your case board.", out _effectId)
                .AddSaleLocation(SyncDiskBuilder.SyncDiskSaleLocation.BlackmarketTrader)
                .CreateAndRegister();
        }

        private void OnSyncDiskUninstall(object sender, SyncDiskArgs e)
        {
            if (e.Effect != null && e.Effect.Value.Id == _effectId)
                Enabled = false;
        }

        private void OnSyncDiskInstall(object sender, SyncDiskArgs e)
        {
            if (e.Effect != null && e.Effect.Value.Id == _effectId)
                Enabled = true;
        }
    }

    // TODO: Add patch to adjust actors when evidence of an actor profile is added or removed to the current caseboard, or when current caseboard is switched.

    /// <summary>
    /// Handles the logic for setting the color of the speech bubble text
    /// </summary>
    [HarmonyPatch(typeof(SpeechController), nameof(SpeechController.Update))]
    internal static class SpeechController_Update
    {
        private static readonly HashSet<int> _actorsOnCurrentCaseboard = new();
        private static bool _speechActive = false, _isDirty = false;
        private static Color _originalColor;

        [HarmonyPostfix]
        internal static void Postfix(SpeechController __instance)
        {
            // Speech active check so we don't keep overdoing the logic each frame
            var speechActive = __instance.activeSpeechBubble != null;
            if (_speechActive != speechActive || _isDirty)
            {
                // If we didn't come from a dirty check, and there are no actors we don't need to do anything
                if (!_isDirty && _actorsOnCurrentCaseboard.Count == 0) 
                    return;

                // Update the latest speech state
                _speechActive = speechActive;

                // Grab the original color when it becomes active
                if (speechActive && !_isDirty)
                    _originalColor = __instance.activeSpeechBubble.text.color;

                // Set the color of the speech bubble if we know the actor from the current case board
                __instance.activeSpeechBubble.text.color = GetColorForActor(__instance.actor);
                _isDirty = false;
            }
        }

        private static Color GetColorForActor(Actor actor)
        {
            // Get cached value based on if actor is on caseboard or not
            return _actorsOnCurrentCaseboard.Contains(actor.ai.human.humanID) ? 
                Color.cyan : _originalColor;
        }

        /// <summary>
        /// Call this method when an evidence not is added or removed that is an actor profile.
        /// </summary>
        /// <param name="actors"></param>
        internal static void AdjustActors(IEnumerable<Actor> actors)
        {
            // Clone the hashset for dirty check
            var prev = _actorsOnCurrentCaseboard.ToHashSet();

            // Attempt to modify the hashset
            _actorsOnCurrentCaseboard.Clear();
            foreach (var actor in actors)
            {
                if (actor.ai != null && actor.ai.human != null)
                {
                    // TODO: Additionally also verify that the actor has voice print identified
                    _actorsOnCurrentCaseboard.Add(actor.ai.human.humanID);
                }
            }

            // Set dirty if the set was modified
            _isDirty = prev.Count != _actorsOnCurrentCaseboard.Count || !prev.All(a => _actorsOnCurrentCaseboard.Contains(a));
        }
    }
}
