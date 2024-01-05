using System;
using System.Collections.Generic;
using System.Linq;

namespace SOD.Common.Helpers.SyncDiskObjects
{
    public sealed class SyncDiskBuilder
    {
        internal string Name { get; private set; }
        internal int Price { get; private set; }
        internal SyncDiskPreset.Rarity Rarity { get; private set; }
        internal SyncDiskPreset.Manufacturer Manufacturer { get; private set; }
        internal List<Effect> Effects { get; private set; }
        internal Effect SideEffect { get; private set; }
        internal HashSet<string> MenuPresetLocations { get; private set; }
        internal bool CanBeSideJobReward { get; private set; }

        internal SyncDiskBuilder(string syncDiskName) 
        {
            Name = syncDiskName;
            Effects = new List<Effect>(3);
            MenuPresetLocations = new HashSet<string>();
        }

        /// <summary>
        /// Sets the price of the sync disk.
        /// </summary>
        /// <param name="price"></param>
        /// <param name="rarity"></param>
        /// <param name="canBeSideJobReward"></param>
        /// <returns></returns>
        public SyncDiskBuilder SetPrice(int price)
        {
            Price = price;
            return this;
        }

        /// <summary>
        /// Sets the rarity of the sync disk.
        /// </summary>
        /// <param name="rarity"></param>
        /// <returns></returns>
        public SyncDiskBuilder SetRarity(SyncDiskPreset.Rarity rarity)
        {
            Rarity = rarity;
            return this;
        }

        /// <summary>
        /// Set's the manufacturer for the sync disk.
        /// </summary>
        /// <param name="manufacturer"></param>
        /// <returns></returns>
        public SyncDiskBuilder SetManufacturer(SyncDiskPreset.Manufacturer manufacturer)
        {
            Manufacturer = manufacturer;
            return this;
        }

        /// <summary>
        /// Sets true/false if this can be retrieved from a side job reward.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public SyncDiskBuilder SetCanBeSideJobReward(bool value)
        {
            CanBeSideJobReward = value;
            return this;
        }

        /// <summary>
        /// Add's a new custom effect to the sync disk, this method can be called up to a max of 3 times.
        /// <br>Setting effect 1, 2, 3</br>
        /// <br>The <paramref name="uniqueEffectId"/> is id, that the SyncDisk events will return on install/uninstall/upgrade.</br>
        /// </summary>
        /// <returns></returns>
        public SyncDiskBuilder AddEffect(string name, string description, out int uniqueEffectId)
        {
            if (Effects.Count == 3)
                throw new Exception("This sync disk already contains 3 effects.");

            // Get a new unique effect id that can be used by mods
            uniqueEffectId = SyncDisks.GetNewSyncDiskEffectId();

            // Create and store the effect
            Effects.Add(new Effect(uniqueEffectId, name, description));

            return this;
        }

        /// <summary>
        /// (Optional) Add's a side effect to the sync disk, this method can only be called once.
        /// <br>The <paramref name="uniqueSideEffectId"/> is id, that the SyncDisk events will return on install/uninstall/upgrade.</br>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="uniqueSideEffectId"></param>
        /// <returns></returns>
        public SyncDiskBuilder AddSideEffect(string name, string description, out int uniqueSideEffectId)
        {
            if (SideEffect != null)
                throw new Exception("This sync disk already has a side effect.");

            // Get a new unique effect id that can be used by mods
            uniqueSideEffectId = SyncDisks.GetNewSyncDiskEffectId();

            // Create and store the side effect
            SideEffect = new Effect(uniqueSideEffectId, name, description);

            return this;
        }

        /// <summary>
        /// Add's locations where the sync disk can be retrieved from.
        /// <br>These are all menu locations.</br>
        /// </summary>
        /// <param name="saleLocations"></param>
        /// <returns></returns>
        public SyncDiskBuilder AddSaleLocation(params RegistrationOptions.SyncDiskSaleLocation[] saleLocations)
        {
            foreach (var saleLocation in saleLocations.Select(a => a.ToString()))
                MenuPresetLocations.Add(saleLocation);
            return this;
        }

        /// <summary>
        /// Add's locations where the sync disk can be retrieved from.
        /// <br>These are all menu locations.</br>
        /// <br>This method is a string variant, which takes the menu presetName.</br>
        /// <br>It exists to set location for a custom menu presets.</br>
        /// </summary>
        /// <param name="saleLocations"></param>
        /// <returns></returns>
        public SyncDiskBuilder AddSaleLocation(params string[] saleLocations)
        {
            foreach (var saleLocation in saleLocations)
                MenuPresetLocations.Add(saleLocation);
            return this;
        }

        /// <summary>
        /// Creates a new Sync Disk based on the current builder configuration.
        /// </summary>
        /// <returns></returns>
        public SyncDisk Create()
        {
            return SyncDisk.ConvertFrom(this);
        }

        /// <summary>
        /// Creates and register the new sync disk based on the current builder configuration.
        /// </summary>
        /// <param name="registrationOptions"></param>
        /// <returns></returns>
        public SyncDisk CreateAndRegister()
        {
            var syncDisk = SyncDisk.ConvertFrom(this);
            syncDisk.Register();
            return syncDisk;
        }

        internal class Effect
        {
            public string Name { get; }
            public string Description { get; }
            public SyncDiskPreset.Effect EffectValue { get; }

            internal Effect(int id, string name, string description)
            {
                EffectValue = (SyncDiskPreset.Effect)id;
                Name = name;
                Description = description;
            }
        }
    }
}
