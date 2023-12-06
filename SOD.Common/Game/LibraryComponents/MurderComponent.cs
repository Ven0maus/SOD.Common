namespace SOD.Common.Game.LibraryComponents
{
    /// <summary>
    /// Component that handles things regarding the Murders.
    /// </summary>
    public class MurderComponent
    {
        /// <summary>
        /// The murder controller.
        /// </summary>
        public MurderController Controller => MurderController.Instance;
        /// <summary>
        /// The latest (last) active murder.
        /// </summary>
        public MurderController.Murder LatestActiveMurder => Controller.activeMurders.Count == 0 ? null : 
            Controller.activeMurders[^1];
    }
}
