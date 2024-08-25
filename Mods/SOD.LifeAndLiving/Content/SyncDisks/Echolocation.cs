using HarmonyLib;
using SOD.Common;
using SOD.Common.Extensions;
using SOD.Common.Helpers.SyncDiskObjects;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Color = UnityEngine.Color;

namespace SOD.LifeAndLiving.Content.SyncDisks
{
    /// <summary>
    /// Add's a sync disk that can recognize the chatter of npcs which are selected on your current selected case board as evidence.
    /// </summary>
    internal static class Echolocation
    {
        private static int _effectId;
        internal static Color Color { get; private set; }

        internal static void Initialize() 
        {
            if (!Plugin.Instance.Config.IncludeEcholocationSyncDisk) return;

            // Read color from config
            try
            {
                var color = ColorTranslator.FromHtml(Plugin.Instance.Config.EcholocationTextColor);
                Color = new Color(color.R, color.G, color.B, color.A);
            }
            catch
            {
                Plugin.Log.LogError("Unable to read config value for \"NpcChatterTextColor\", using fallback cyan color.");
                Color = Color.cyan;
            }

            Lib.SyncDisks.OnAfterSyncDiskInstalled += OnSyncDiskInstall;
            Lib.SyncDisks.OnAfterSyncDiskUninstalled += OnSyncDiskUninstall;

            // Create and register the sync disk
            _ = Lib.SyncDisks.Builder("Echolocation", Plugin.PLUGIN_GUID)
                .SetPrice(Plugin.Instance.Config.EcholocationDiskPrice)
                .SetManufacturer(SyncDiskPreset.Manufacturer.BlackMarket)
                .SetRarity(SyncDiskPreset.Rarity.veryRare)
                .AddEffect("Echolocation", "Highlights civilian chatter for your selected case if their voice is identified and their profile is on your case board.", out _effectId)
                .AddSaleLocation(new[] { SyncDiskBuilder.SyncDiskSaleLocation.BlackmarketTrader })
                .CreateAndRegister();
        }

        private static void OnSyncDiskUninstall(object sender, SyncDiskArgs e)
        {
            if (e.Effect != null && e.Effect.Value.Id == _effectId)
            {
                SpeechController_Update.EffectEnabled = false;
                SpeechController_Update.ClearActors();
            }
        }

        private static void OnSyncDiskInstall(object sender, SyncDiskArgs e)
        {
            if (e.Effect != null && e.Effect.Value.Id == _effectId)
            {
                SpeechController_Update.EffectEnabled = true;
                SpeechController_Update.ClearActors();
                SpeechController_Update.SetActorsOfCurrentCase();
            }
        }
    }

    [HarmonyPatch(typeof(CasePanelController), nameof(CasePanelController.UpdatePinned))]
    internal static class CasePanelController_UpdatePinned
    {
        [HarmonyPostfix]
        internal static void Postfix(CasePanelController __instance)
        {
            if (MainMenuController.Instance.mainMenuActive || __instance.activeCase == null)
                return;
            SpeechController_Update.SetActorsOfCurrentCase();
        }
    }

    [HarmonyPatch(typeof(CasePanelController), nameof(CasePanelController.SetActiveCase))]
    internal static class CasePanelController_SetActiveCase
    {
        private static Case _currentCase;

        [HarmonyPrefix]
        internal static void Prefix(CasePanelController __instance)
        {
            _currentCase = __instance.activeCase;
        }

        [HarmonyPostfix]
        internal static void Postfix(CasePanelController __instance)
        {
            if (_currentCase == null && __instance.activeCase == null) return;
            if ((_currentCase == null && __instance.activeCase != null) ||
                (_currentCase != null && __instance.activeCase == null) ||
                (_currentCase.id != __instance.activeCase.id))
            {
                SpeechController_Update.ClearActors();
                SpeechController_Update.SetActorsOfCurrentCase();
            }
        }
    }

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

                // Nothing to do when there is no active speech bubble
                if (__instance.activeSpeechBubble == null)
                    return;

                // Grab the original color when it becomes active
                if (speechActive && !IsDirty)
                _originalColor = __instance.activeSpeechBubble.text.color;

                // Set the color of the speech bubble if we know the actor from the current case board
                __instance.activeSpeechBubble.text.color = GetColorForActor(__instance.actor);
                IsDirty = false;
            }
        }

        private static readonly HashSet<int> _empty = new();

        /// <summary>
        /// Call this when you want to reset all the colors to default.
        /// </summary>
        internal static void ClearActors()
        {
            _actorsOnCurrentCaseboard.Clear();
            IsDirty = true;
        }

        /// <summary>
        /// Collects all the valid actors on the current active case board with voice identified.
        /// </summary>
        /// <returns></returns>
        internal static void SetActorsOfCurrentCase()
        {
            if (CasePanelController.Instance.activeCase == null)
            {
                AdjustActors(_empty);
                return;
            }

            HashSet<int> data = null;
            foreach (var caseElement in CasePanelController.Instance.activeCase.caseElements)
            {
                if (Toolbox.Instance.TryGetEvidence(caseElement.id, out var evidence))
                {
                    var evidenceCitizen = evidence.TryCast<EvidenceCitizen>();
                    if (evidenceCitizen == null) continue;
                    
                    // Get tied keys of the caseElement
                    var tiedKeys = evidence.GetTiedKeys(caseElement.dk);

                    // Check if voice is enabled
                    if (tiedKeys.Contains(Evidence.DataKey.voice))
                    {
                        data ??= new HashSet<int>();
                        data.Add(evidenceCitizen.witnessController.humanID);
                    }
                }
            }

            AdjustActors(data ?? _empty);
        }

        private static Color GetColorForActor(Actor actor)
        {
            // Get cached value based on if actor is on caseboard or not
            return EffectEnabled && actor.ai != null && actor.ai.human != null && _actorsOnCurrentCaseboard.Contains(actor.ai.human.humanID) ?
                Echolocation.Color : _originalColor;
        }

        private static void AdjustActors(HashSet<int> actorIds)
        {
            // Don't do anything when nothing changed
            if (_actorsOnCurrentCaseboard.Count == 0 && actorIds.Count == 0)
                return;

            // Clone the hashset for dirty check
            var prev = _actorsOnCurrentCaseboard.ToHashSet();

            // Attempt to modify the hashset
            _actorsOnCurrentCaseboard.Clear();
            foreach (var actorId in actorIds)
                _actorsOnCurrentCaseboard.Add(actorId);

            // Set dirty if the set was modified
            IsDirty = prev.Count != _actorsOnCurrentCaseboard.Count || !prev.All(a => _actorsOnCurrentCaseboard.Contains(a));
        }
    }
}
