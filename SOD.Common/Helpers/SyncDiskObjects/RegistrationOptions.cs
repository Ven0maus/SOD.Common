namespace SOD.Common.Helpers.SyncDiskObjects
{
    public sealed class RegistrationOptions
    {
        internal RegistrationOptions() { }

        /// <summary>
        /// Can this sync disk be rewarded from a side job?
        /// </summary>
        public bool CanBeSideJobReward { get; } = true;
    }
}
