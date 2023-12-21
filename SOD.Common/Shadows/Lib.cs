using SOD.Common.Shadows.Implementations;

namespace SOD.Common.Shadows
{
    /// <summary>
    /// Base library that provides all helper implementations.
    /// </summary>
    public class Lib
    {
        /// <summary>
        /// Contains helpers to broadcast messages in game.
        /// </summary>
        public static GameMessage GameMessage { get; } = new GameMessage();

        /// <summary>
        /// Contains helper methods regarding game time.
        /// </summary>
        public static Time Time { get; } = new Time();

        /// <summary>
        /// Contains helper methods regarding saving and loading game.
        /// </summary>
        public static SaveGame SaveGame { get; } = new SaveGame();

        /// <summary>
        /// Contains helper methods for converting to Il2Cpp objects
        /// </summary>
        public static Il2CppConverter Il2Cpp { get; } = new Il2CppConverter();
    }
}
