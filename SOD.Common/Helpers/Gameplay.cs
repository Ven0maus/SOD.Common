﻿using SOD.Common.Extensions;
using SOD.Common.Helpers.GameplayObjects;
using System;
using System.Linq;

namespace SOD.Common.Helpers
{
    public sealed class Gameplay
    {
        internal Gameplay() { }

        /// <summary>
        /// Raises when a new victim is picked by the murder controller logic.
        /// </summary>
        public event EventHandler<HumanPickedArgs> OnVictimPicked;
        /// <summary>
        /// Raises when a new murderer is picked by the murder controller logic.
        /// </summary>
        public event EventHandler<HumanPickedArgs> OnMurdererPicked;
        /// <summary>
        /// Raises when a victim is reported by a civilian, and provides some arguments related to this.
        /// </summary>
        public event EventHandler<VictimReportedArgs> OnVictimReported;
        /// <summary>
        /// Raises when a victim is killed by the murderer, and provides some arguments related to this.
        /// </summary>
        public event EventHandler<VictimKilledArgs> OnVictimKilled;
        /// <summary>
        /// Raises when an interactable is picked up by the player.
        /// </summary>
        public event EventHandler<InteractableArgs> OnInteractablePickup;
        /// <summary>
        /// Raises when an interactable is dropped or thrown by the player.
        /// </summary>
        public event EventHandler<InteractableArgs> OnInteractableDropped;

        /// <summary>
        /// Checks if an airvent exist in a house, returns true/false and outs the airvents found.
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public bool AirVentExistsInHouse(NewAddress address, out AirDuctGroup.AirVent[] airVents)
        {
            airVents = address.rooms
                .AsEnumerable()
                .SelectMany(a => a.airVents.AsEnumerable())
                .ToArray();
            return airVents.Length > 0;
        }

        /// <summary>
        /// Checks if a certain interactable exists in a house, returns true/false and outs the interactables found.
        /// </summary>
        /// <param name="interactablePresetName"></param>
        /// <param name="address"></param>
        /// <param name="interactables"></param>
        /// <returns></returns>
        public bool InteractableExistsInHouse(string interactablePresetName, NewAddress address, out Interactable[] interactables)
        {
            if (!Toolbox.Instance.objectPresetDictionary.TryGetValue(interactablePresetName, out var preset))
                throw new Exception($"No interactable preset exists by name \"{interactablePresetName}\".");

            interactables = address.nodes
                .AsEnumerable()
                .SelectMany(a => a.interactables.AsEnumerable())
                .Where(b => b.preset.name.Equals(preset.name))
                .ToArray();
            return interactables.Length != 0;
        }

        /// <summary>
        /// Add's a new interactable to a house.
        /// </summary>
        /// <param name="interactablePresetName"></param>
        /// <param name="address"></param>
        /// <param name="furnitureLocation"></param>
        /// <param name="interactable"></param>
        /// <param name="belongsTo"></param>
        /// <param name="writer"></param>
        /// <param name="receiver"></param>
        /// <param name="placementRule"></param>
        /// <exception cref="Exception"></exception>
        public void AddInteractableToHouse(string interactablePresetName, NewAddress address, 
            out FurnitureLocation furnitureLocation, out Interactable interactable,
            Human belongsTo = null, Human writer = null, Human receiver = null, InteractablePreset.OwnedPlacementRule placementRule = InteractablePreset.OwnedPlacementRule.nonOwnedOnly)
        {
            if (!Toolbox.Instance.objectPresetDictionary.TryGetValue(interactablePresetName, out var preset))
                throw new Exception($"No interactable preset exists by name \"{interactablePresetName}\".");

            interactable = address.PlaceObject(preset, belongsTo, writer, receiver, out furnitureLocation, false, 
                Interactable.PassedVarType.jobID, -1, true, 0, placementRule, 0, null, false, null, null, null, "", true);
        }

        /// <summary>
        /// Get's an interactable preset by name.
        /// </summary>
        /// <param name="presetName"></param>
        /// <returns></returns>
        public InteractablePreset GetInteractablePresetByName(string presetName)
        {
            if (!Toolbox.Instance.objectPresetDictionary.TryGetValue(presetName, out var preset))
                return null;
            return preset;
        }

        /// <summary>
        /// Check's if the player has a certain interactable in their inventory and returns them if available.
        /// </summary>
        /// <param name="interactablePresetName"></param>
        /// <param name="interactables"></param>
        /// <returns></returns>
        public bool HasInteractableInInventory(string interactablePresetName, out Interactable[] interactables)
        {
            // Check if we have a screwdriver in our inventory
            interactables = FirstPersonItemController.Instance.slots.ToList()
                .Select(a =>
                {
                    if (a.interactableID == -1) return null;
                    var inter = a.GetInteractable();
                    if (inter == null || inter.preset == null || inter.preset.presetName == null) return null;
                    return inter;
                })
                .Where(a => a != null && a.preset.presetName.Equals(interactablePresetName))
                .ToArray();
            return interactables.Length != 0;
        }

        internal void VictimReported(Human victim, Human reporter, Human.Death.ReportType reportType)
        {
            if (Plugin.InDebugMode)
                Plugin.Log.LogInfo($"[DebugMode]: Victim reported: ({victim.humanID}) {victim.citizenName} | Reporter: ({reporter.humanID}) {reporter.citizenName}");
            OnVictimReported?.Invoke(this, new VictimReportedArgs(victim, reporter, reportType));
        }

        internal void VictimKilled(MurderController.Murder murder)
        {
            if (Plugin.InDebugMode)
                Plugin.Log.LogInfo($"[DebugMode]: Victim killed: ({murder.victimID}) {murder.victim.citizenName}");
            OnVictimKilled?.Invoke(this, new VictimKilledArgs(murder));
        }

        internal void VictimPicked(Human previous, Human @new)
        {
            if (Plugin.InDebugMode)
                Plugin.Log.LogInfo($"[DebugMode]: New victim picked: ({@new.humanID}) {@new.citizenName}");
            OnVictimPicked?.Invoke(this, new HumanPickedArgs(previous, @new));
        }

        internal void MurdererPicked(Human previous, Human @new)
        {
            if (Plugin.InDebugMode)
                Plugin.Log.LogInfo($"[DebugMode]: New murderer picked: ({@new.humanID}) {@new.citizenName}");
            OnMurdererPicked?.Invoke(this, new HumanPickedArgs(previous, @new));
        }

        internal void InteractablePickedUp(Interactable interactable)
        {
            if (Plugin.InDebugMode)
                Plugin.Log.LogInfo($"[DebugMode]: Picked up interactable: ({interactable.id}) {interactable.name}");
            OnInteractablePickup?.Invoke(this, new InteractableArgs(interactable));
        }

        internal void InteractableDropped(Interactable interactable, bool wasThrown)
        {
            if (Plugin.InDebugMode)
                Plugin.Log.LogInfo($"[DebugMode]: {(wasThrown ? "Threw" : "Picked up")} interactable: ({interactable.id}) {interactable.name}");
            OnInteractableDropped?.Invoke(this, new InteractableArgs(interactable, wasThrown));
        }
    }
}
