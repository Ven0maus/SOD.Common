using SOD.Common.Extensions;
using System.Linq;
using UnityEngine;

namespace SOD.Common.Game.LibraryComponents
{
    public sealed class SyncDiskComponent
    {
        internal SyncDiskComponent() { }

        /// <summary>
        /// All sync disk presets, add your preset here in ToolBox.LoadAll hook
        /// </summary>
        public Il2CppSystem.Collections.Generic.List<SyncDiskPreset> AllSyncDiskPresets => Toolbox.Instance.allSyncDisks;

        /// <summary>
        /// Creates a new sync disk preset which can be used to instantiate new sync disks.
        /// <br>If you want it to be able to spawn in the world, you must add it to <see cref="AllSyncDiskPresets"/> before world generation.</br>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="diskNumber"></param>
        /// <param name="price"></param>
        /// <param name="canBeSideJobReward"></param>
        /// <param name="rarity"></param>
        /// <param name="manufacturer"></param>
        /// <returns></returns>
        public SyncDiskPreset CreateSyncDiskPreset(string name, int diskNumber, int price, bool canBeSideJobReward, SyncDiskPreset.Rarity rarity, SyncDiskPreset.Manufacturer manufacturer)
        {
            var preset = ScriptableObject.CreateInstance<SyncDiskPreset>();

            preset.presetName = name;
            preset.name = preset.presetName;
            preset.price = price;
            preset.rarity = rarity;
            preset.canBeSideJobReward = canBeSideJobReward;
            preset.manufacturer = manufacturer;

            // TODO: Optimize
            preset.syncDiskNumber = diskNumber;

            // TODO: Optimize
            preset.interactable = Resources.FindObjectsOfTypeAll<InteractablePreset>()
                .LastOrDefault(preset => preset.presetName == "SyncDisk");

            return preset;
        }

        /// <summary>
        /// Adds the sync disk preset to the police vending machines
        /// </summary>
        /// <param name="syncDiskPreset"></param>
        public void AddToEnforcerVendingMachines(params SyncDiskPreset[] syncDiskPresets)
        {
            // TODO: Optimize
            Resources.FindObjectsOfTypeAll<MenuPreset>()
                .Where(menuPreset => menuPreset.presetName == "PoliceAutomat")
                .ForEach(machine => syncDiskPresets
                .ForEach(syncDiskPreset => machine.syncDisks.Add(syncDiskPreset)));
        }
    }
}
