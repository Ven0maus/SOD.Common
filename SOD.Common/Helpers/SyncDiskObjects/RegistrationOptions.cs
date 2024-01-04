namespace SOD.Common.Helpers.SyncDiskObjects
{
    public sealed class RegistrationOptions
    {
        internal RegistrationOptions() { }

        /// <summary>
        /// Makes it so the sync disk can be generated in the world randomly.
        /// <br>Default: true</br>
        /// </summary>
        public bool AddToWorldGeneration { get; set; } = true;
    }
}
