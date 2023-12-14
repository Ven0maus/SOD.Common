using SOD.Common.Shadows.Implementations;

namespace SOD.Common.Shadows
{
    public class Common
    {
        /// <summary>
        /// Provides easy access to broadcasting game messages.
        /// </summary>
        public static GameMessage GameMessage { get; } = new GameMessage();
    }
}
