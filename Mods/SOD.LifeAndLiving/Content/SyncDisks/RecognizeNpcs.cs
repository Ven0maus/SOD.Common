using HarmonyLib;
using SOD.Common;
using SOD.Common.Extensions;
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
            {
                SpeechController_Update.EffectEnabled = false;
                SpeechController_Update.IsDirty = true;
            }
        }

        private void OnSyncDiskInstall(object sender, SyncDiskArgs e)
        {
            if (e.Effect != null && e.Effect.Value.Id == _effectId)
            {
                SpeechController_Update.EffectEnabled = true;
                SpeechController_Update.IsDirty = true;
            }
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
        private static bool _speechActive = false;
        private static Color _originalColor;

        internal static bool IsDirty = false, EffectEnabled = false;

        [HarmonyPostfix]
        internal static void Postfix(SpeechController __instance)
        {
            // Speech active check so we don't keep overdoing the logic each frame
            var speechActive = __instance.activeSpeechBubble != null;
            if (_speechActive != speechActive || IsDirty)
            {
                // If we didn't come from a dirty check, and there are no actors or effect not enabled we don't need to do anything
                if (!IsDirty && (!EffectEnabled || _actorsOnCurrentCaseboard.Count == 0)) 
                    return;

                // Update the latest speech state
                _speechActive = speechActive;

                // Grab the original color when it becomes active
                if (speechActive && !IsDirty)
                    _originalColor = __instance.activeSpeechBubble.text.color;

                // Set the color of the speech bubble if we know the actor from the current case board
                __instance.activeSpeechBubble.text.color = GetColorForActor(__instance.actor);
                IsDirty = false;
            }
        }

        private static Color GetColorForActor(Actor actor)
        {
            // Get cached value based on if actor is on caseboard or not
            return EffectEnabled && _actorsOnCurrentCaseboard.Contains(actor.ai.human.humanID) ? 
                Color.cyan : _originalColor;
        }

        /// <summary>
        /// Call this method when an evidence not is added or removed that is an actor profile.
        /// </summary>
        /// <param name="actorIds"></param>
        internal static void AdjustActors(IEnumerable<int> actorIds)
        {
            // Clone the hashset for dirty check
            var prev = _actorsOnCurrentCaseboard.ToHashSet();

            // Attempt to modify the hashset
            _actorsOnCurrentCaseboard.Clear();
            foreach (var actorId in actorIds)
                _actorsOnCurrentCaseboard.Add(actorId);

            // Set dirty if the set was modified
            IsDirty = prev.Count != _actorsOnCurrentCaseboard.Count || !prev.All(a => _actorsOnCurrentCaseboard.Contains(a));
        }

        /// <summary>
        /// Collects all the valid actors on the current active case board with voice identified.
        /// </summary>
        /// <returns></returns>
        internal static HashSet<int> CollectActorsOnCurrentCaseWithVoiceUnlocked()
        {
            var data = new HashSet<int>();
            if (CasePanelController.Instance.activeCase == null)
                return data;

            // Add tied keys to find voice, figure out which keys we actually need to check for
            var tiedKeys = new Il2CppSystem.Collections.Generic.List<Evidence.DataKey>();
            tiedKeys.Add(Evidence.DataKey.photo);
            tiedKeys.Add(Evidence.DataKey.name);

            foreach (var caseElement in CasePanelController.Instance.activeCase.caseElements)
            {
                if (Toolbox.Instance.TryGetEvidence(caseElement.id, out var evidence))
                {
                    var evidenceCitizen = evidence.TryCast<EvidenceCitizen>();
                    if (evidenceCitizen == null) continue;

                    // Check if voice is enabled
                    // TODO: Fix this check
                    if (evidence.GetTiedKeys(tiedKeys).Contains(Evidence.DataKey.voice))
                    {
                        Plugin.Log.LogInfo("Found citizen evidence for citizen: " + evidenceCitizen.witnessController.humanID);
                        data.Add(evidenceCitizen.witnessController.humanID);
                    }
                }
            }

            return data;
        }
    }
}
