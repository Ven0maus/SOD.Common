using SOD.Common.Shadows.Implementations;

namespace SOD.Common.Shadows
{
    public class Lib
    {
        /// <summary>
        /// Provides easy access to broadcasting game messages.
        /// </summary>
        public static GameMessage GameMessage { get; } = new GameMessage();

        /// <summary>
        /// Contains game time helper methods
        /// </summary>
        public static Time Time { get; } = new Time();
    }
}
