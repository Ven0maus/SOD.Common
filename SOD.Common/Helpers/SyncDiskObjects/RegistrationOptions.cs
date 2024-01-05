using System.Collections.Generic;

namespace SOD.Common.Helpers.SyncDiskObjects
{
    public sealed class RegistrationOptions
    {
        public RegistrationOptions() { }

        /// <summary>
        /// Can this sync disk be rewarded from a side job?
        /// </summary>
        public bool CanBeSideJobReward { get; set; } = true;

        /// <summary>
        /// All the locations you want the sync disk to be retrievable from.
        /// </summary>
        public List<SyncDiskSaleLocation> SaleLocations { get; set; }

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
        }
    }
}
