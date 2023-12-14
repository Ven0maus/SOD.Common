namespace SOD.Common.Shadows.Implementations
{
    public sealed class GameMessage
    {
        /// <summary>
        /// Broadcasts a message to be shown on the screen.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="messageType"></param>
        /// <param name="icon"></param>
        /// <param name="color"></param>
        /// <param name="delay"></param>
        public void Broadcast(string message, InterfaceController.GameMessageType messageType = InterfaceController.GameMessageType.notification, InterfaceControls.Icon icon = InterfaceControls.Icon.empty, UnityEngine.Color? color = null, float delay = 0f)
        {
            if (color == null)
                color = InterfaceControls.Instance.messageRed;
            InterfaceController.Instance.NewGameMessage(
                newType: messageType,
                newNumerical: 0,
                newMessage: message,
                newIcon: icon,
                colourOverride: true,
                col: color.Value,
                newMessageDelay: delay);
        }
    }
}
