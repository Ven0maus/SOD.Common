using System;

namespace SOD.Common.Helpers.SyncDiskObjects
{
    public sealed class SyncDiskArgs : EventArgs
    {
        /// <summary>
        /// This is considered the change applied to the sync disk object that was installed/upgraded/uninstalled
        /// </summary>
        public UpgradesController.Upgrades SyncDiskChange { get; }

        /// <summary>
        /// The sync disk wrapper related to this event.
        /// </summary>
        public SyncDisk SyncDisk { get; }

        /// <summary>
        /// The sync disk's number.
        /// </summary>
        public int Number => SyncDisk.Number;

        /// <summary>
        /// The effect of the sync disk related to this event.
        /// <br>This will be one of the 3 main effects.</br>
        /// </summary>
        public SyncDisks.Effect? Effect { get; }

        /// <summary>
        /// The upgrade option of the sync disk related to this event.
        /// <br>This will be one of the 3 main upgrade options.</br>
        /// </summary>
        public Option? UpgradeOption { get; }

        internal SyncDiskArgs(UpgradesController.Upgrades upgrades, bool setEffect = true, bool setUpgradeOption = true)
        {
            SyncDiskChange = upgrades;
            SyncDisk = SyncDisk.ConvertFrom(upgrades.preset);

            var effect = (int)upgrades.state;
            if (setEffect)
            {
                if (effect == 0)
                {
                    Effect = null;
                }
                else
                {
                    var realEffectArrayId = effect - 1;
                    if (SyncDisk.Effects.Length > realEffectArrayId)
                        Effect = SyncDisk.Effects[realEffectArrayId];
                }
            }

            if (setUpgradeOption)
            {
                // Set the option properly
                var option = upgrades.level;
                if (option == 0 || effect == 0)
                {
                    UpgradeOption = null;
                }
                else
                {
                    // Here we use effect as the option comes from the effect
                    var realOptionArrayId = effect - 1;
                    if (SyncDisk.UpgradeOptions.Length > realOptionArrayId)
                    {
                        var selectedOption = SyncDisk.UpgradeOptions[realOptionArrayId];
                        if (option == 1)
                            UpgradeOption = new Option(selectedOption.Id1, selectedOption.Name1);
                        else if (option == 2)
                            UpgradeOption = new Option(selectedOption.Id2, selectedOption.Name2);
                        else if (option == 3)
                            UpgradeOption = new Option(selectedOption.Id3, selectedOption.Name3);
                    }
                }
            }
        }

        public readonly struct Option
        {
            public readonly int Id;
            public readonly string Name;

            public Option(int id, string name)
            {
                Id = id;
                Name = name;
            }
        }
    }
}
