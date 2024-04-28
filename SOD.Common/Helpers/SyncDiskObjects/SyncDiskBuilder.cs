using System;
using System.Collections.Generic;
using System.Linq;

namespace SOD.Common.Helpers.SyncDiskObjects
{
    public sealed class SyncDiskBuilder
    {
        internal string Name { get; private set; }
        internal string PluginGuid { get; private set; }
        internal bool ReRaiseEventsOnSaveLoad { get; private set; }
        internal int Price { get; private set; }
        internal SyncDiskPreset.Rarity Rarity { get; private set; }
        internal SyncDiskPreset.Manufacturer Manufacturer { get; private set; }
        internal List<Effect> Effects { get; private set; }
        internal Effect SideEffect { get; private set; }
        internal List<Options> UpgradeOptions { get; private set; }
        internal HashSet<string> MenuPresetLocations { get; private set; }
        internal OccupationPreset[] OccupationPresets { get; private set; }
        internal SyncDiskPreset.TraitPick[] Traits { get; private set; }
        internal int OccupationWeight { get; private set; }
        internal int TraitWeight { get; private set; }
        internal bool CanBeSideJobReward { get; private set; }
        internal bool EnableWorldSpawn { get; private set; } = true;

        internal SyncDiskBuilder(string syncDiskName, string pluginGuid, bool reRaiseEventsOnSaveLoad = true)
        {
            Name = syncDiskName;
            PluginGuid = pluginGuid;
            ReRaiseEventsOnSaveLoad = reRaiseEventsOnSaveLoad;
            Effects = new List<Effect>(3);
            UpgradeOptions = new List<Options>(3);
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
        /// Determines if the sync disk can be spawned within the world automatically by world generation.
        /// <br>If disable, it will only be spawned by setting the Sale location, or manually by your own code logic.</br>
        /// <br>If enabled, you will have to assign occupation or traits to the preset for it to spawn in the world.</br>
        /// </summary>
        /// <param name="enabled"></param>
        /// <returns></returns>
        public SyncDiskBuilder SetWorldSpawnOption(bool enabled)
        {
            EnableWorldSpawn = enabled;
            return this;
        }

        /// <summary>
        /// Sets spawn chance in the world for certain occupations (will spawn in their buildings.)
        /// <br>Weight should be atleast one, but it affects the chance of selection</br>
        /// </summary>
        /// <param name="presets"></param>
        /// <returns></returns>
        public SyncDiskBuilder SetOccupations(int weight, params OccupationPreset[] presets)
        {
            OccupationPresets = presets;
            OccupationWeight = weight;
            return this;
        }

        /// <summary>
        /// Sets spawn chance in the world for certain traits (will spawn in their houses)
        /// <br>Weight should be atleast one, but it affects the chance of selection</br>
        /// </summary>
        /// <returns></returns>
        public SyncDiskBuilder SetTraits(int weight, params SyncDiskPreset.TraitPick[] traits)
        {
            Traits = traits;
            TraitWeight = weight;
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
        /// <br>The <paramref name="uniqueEffectId"/> is the id, that the SyncDisk events will return on install/uninstall/upgrade.</br>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="uniqueEffectId"></param>
        /// <param name="iconSpriteName">A valid sprite name to be used from the game.</param>
        /// <returns></returns>
        public SyncDiskBuilder AddEffect(string name, string description, out int uniqueEffectId, string iconSpriteName = null)
        {
            if (Effects.Count == 3)
                throw new Exception("This sync disk already contains 3 effects.");

            // Get a new unique effect id that can be used by mods
            uniqueEffectId = SyncDisks.GetNewSyncDiskEffectId();

            // Create and store the effect
            Effects.Add(new Effect(uniqueEffectId, name, description, iconSpriteName));

            return this;
        }

        /// <summary>
        /// (Optional) Add's a side effect to the sync disk, this method can only be called once.
        /// <br>The <paramref name="uniqueSideEffectId"/> is the id, that the SyncDisk events will return on install/uninstall/upgrade.</br>
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
            SideEffect = new Effect(uniqueSideEffectId, name, description, null);

            return this;
        }

        /// <summary>
        /// Add's a new custom upgrade option to the sync disk, this method can be called up to a max of 3 times.
        /// <br>Setting upgrade options for effect 1, 2, 3</br>
        /// <br>The <paramref name="uniqueOptionIds"/> are the ids, that the SyncDisk events will return on install/uninstall/upgrade.</br>
        /// </summary>
        /// <param name="options"></param>
        /// <param name="uniqueOptionIds"></param>
        /// <returns></returns>
        public SyncDiskBuilder AddUpgradeOption(Options options, out OptionIds uniqueOptionIds)
        {
            if (UpgradeOptions.Count == 3)
                throw new Exception("This sync disk already contains 3 upgrade options.");

            // Get a new unique effect id that can be used by mods
            uniqueOptionIds = new OptionIds(options);

            // Create and store the effect
            UpgradeOptions.Add(options);

            return this;
        }

        /// <summary>
        /// Add's locations where the sync disk can be retrieved from.
        /// <br>These are all menu locations.</br>
        /// </summary>
        /// <param name="saleLocations"></param>
        /// <returns></returns>
        public SyncDiskBuilder AddSaleLocation(params SyncDiskSaleLocation[] saleLocations)
        {
            foreach (var saleLocation in saleLocations.Select(a => a.ToString()))
                MenuPresetLocations.Add(saleLocation);
            return this;
        }

        /// <summary>
        /// Add's locations where the sync disk can be retrieved from.
        /// <br>These are all menu locations.</br>
        /// <br>This method is a string variant, which takes the menu presetName.</br>
        /// <br>It exists to set location for custom menu presets.</br>
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
            public string Icon { get; }
            public SyncDiskPreset.Effect EffectValue { get; }

            internal const string DefaultSprite = "IconDNA";

            internal Effect(int id, string name, string description, string icon)
            {
                EffectValue = (SyncDiskPreset.Effect)id;
                Name = name;
                Description = description;
                Icon = !string.IsNullOrWhiteSpace(icon) ? icon : DefaultSprite;
            }
        }

        public struct Options
        {
            public readonly string Option1, Option2, Option3;
            internal SyncDiskPreset.UpgradeEffect Option1Effect, Option2Effect, Option3Effect;

            internal Options(SyncDiskPreset preset, int count)
            {
                var nameReferences = count == 1 ? preset.option1UpgradeNameReferences : count == 2 ? preset.option2UpgradeNameReferences : preset.option3UpgradeNameReferences;
                var effects = count == 1 ? preset.option1UpgradeEffects : count == 2 ? preset.option2UpgradeEffects : preset.option3UpgradeEffects;

                if (nameReferences.Count >= 1)
                {
                    Option1 = nameReferences[0];
                    Option1Effect = effects[0];
                }
                if (nameReferences.Count >= 2)
                {
                    Option2 = nameReferences[1];
                    Option2Effect = effects[1];
                }
                if (nameReferences.Count == 3)
                {
                    Option3 = nameReferences[2];
                    Option3Effect = effects[2];
                }
            }

            public Options(string option1) { Option1 = option1; }
            public Options(string option1, string option2) : this(option1) { Option2 = option2; }
            public Options(string option1, string option2, string option3) : this(option1, option2) { Option3 = option3; }
        }

        public readonly struct OptionIds
        {
            public readonly int Option1Id, Option2Id, Option3Id;

            internal OptionIds(Options options)
            {
                if (!string.IsNullOrWhiteSpace(options.Option1))
                {
                    Option1Id = SyncDisks.GetNewSyncDiskOptionId();
                    options.Option1Effect = (SyncDiskPreset.UpgradeEffect)Option1Id;
                }
                if (!string.IsNullOrWhiteSpace(options.Option2))
                {
                    Option2Id = SyncDisks.GetNewSyncDiskOptionId();
                    options.Option2Effect = (SyncDiskPreset.UpgradeEffect)Option2Id;
                }
                if (!string.IsNullOrWhiteSpace(options.Option3))
                {
                    Option3Id = SyncDisks.GetNewSyncDiskOptionId();
                    options.Option3Effect = (SyncDiskPreset.UpgradeEffect)Option3Id;
                }
            }
        }

        public enum SyncDiskSaleLocation
        {
            CigaretteMachine,
            AmericanDiner,
            SupermarketFridge,
            CoffeeMachine,
            BlackmarketSyncClinic,
            StreetVendorSnacks,
            ElGenMachine,
            PawnShop,
            KolaMachine,
            SupermarketMagazines,
            AmericanBar,
            SupermarketFruit,
            Chinese,
            Newsstand,
            Supermarket,
            Chemist,
            Hardware,
            SupermarketDrinksCooler,
            SupermarketShelf,
            PoliceAutomat,
            SupermarketFreezer,
            HomeCoffee,
            NewspaperBox,
            BlackmarketTrader,
            WeaponsDealer,
            SyncClinic,
            LoanShark
        }
    }
}
