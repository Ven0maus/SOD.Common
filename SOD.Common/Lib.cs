using SOD.Common.Helpers;

namespace SOD.Common
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
        /// Contains helper methods regarding input detection through Rewired.
        /// </summary>
        public static InputDetection InputDetection { get; } = new InputDetection();

        /// <summary>
        /// Contains helper methods regarding player interactions with objects in the world.
        /// </summary>
        public static Interaction Interaction { get; } = new Interaction();

        /// <summary>
        /// Contains helper methods regarding sync disks.
        /// </summary>
        public static SyncDisks SyncDisks { get; } = new SyncDisks();

        /// <summary>
        /// Contains helper methods and indexers regarding in-game DdsStrings.
        /// </summary>
        public static DdsStrings DdsStrings { get; } = new DdsStrings();

        /// <summary>
        /// Contains helper methods regarding player to npc dialog.
        /// </summary>
        public static Dialog Dialogs { get; } = new Dialog();

        /// <summary>
        /// Contains helpers to detect loaded BepInEx plugins and their config settings.
        /// </summary>
        public static PluginDetection PluginDetection { get; } = new PluginDetection();

        /// <summary>
        /// Contains helper methods regarding in-game player statuses such as illegal status.
        /// </summary>
        public static PlayerStatus PlayerStatus { get; } = new PlayerStatus();

        /// <summary>
        /// Contains helpers to access engine functionality that is stripped from the game.
        /// </summary>
        public static RestoredUnityEngine RestoredEngineFunctions { get; } = new RestoredUnityEngine();

        /// <summary>
        /// Provides several helpful events and tools relative to the dective gameplay.
        /// </summary>
        public static Gameplay Gameplay { get; } = new Gameplay();

        /// <summary>
        /// Contains helpers to create and hook new objectives for cases.
        /// </summary>
        public static CaseObjectives CaseObjectives { get; } = new CaseObjectives();
    }
}
