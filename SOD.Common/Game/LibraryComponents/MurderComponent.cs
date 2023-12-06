namespace SOD.Common.Game.LibraryComponents
{
    /// <summary>
    /// Component that handles things regarding the Murders.
    /// </summary>
    public class MurderComponent
    {
        public int ActiveMurders { get { return MurderController.Instance.activeMurders.Count; } }
    }
}
