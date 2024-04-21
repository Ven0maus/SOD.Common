using System;
using System.IO;
using System.Reflection;

namespace SOD.Common.Helpers
{
    public sealed class SaveGame
    {
        internal SaveGame() { }

        /// <summary>
        /// Raised right before a savegame is saved.
        /// <br>Note: StateSaveData is unavailable in the before event.</br>
        /// </summary>
        public event EventHandler<SaveGameArgs> OnBeforeSave;
        /// <summary>
        /// Raised right after a savegame is saved.
        /// </summary>
        public event EventHandler<SaveGameArgs> OnAfterSave;
        /// <summary>
        /// Raised right before a savegame is loaded.
        /// </summary>
        public event EventHandler<SaveGameArgs> OnBeforeLoad;
        /// <summary>
        /// Raised right after a savegame is loaded.
        /// </summary>
        public event EventHandler<SaveGameArgs> OnAfterLoad;
        /// <summary>
        /// Raised right before a savegame file is removed.
        /// </summary>
        public event EventHandler<SaveGameArgs> OnBeforeDelete;
        /// <summary>
        /// Raised right after a savegame file is removed.
        /// </summary>
        public event EventHandler<SaveGameArgs> OnAfterDelete;
        /// <summary>
        /// Raised right before the game begins to create a new game, before the player clicks on the New Game button in the main menu.
        /// </summary>
        public event EventHandler OnBeforeNewGame;
        /// <summary>
        /// Raised right after the game spawns the player in and hides the loading screen. Only applies to a new game, and not a loaded save game.
        /// </summary>
        public event EventHandler OnAfterNewGame;

        /// <summary>
        /// Returns a unique hashed string related to the savegame. 
        /// <br>Will always return the same unique code for the same <see cref="StateSaveData"/>.</br>
        /// <br>Can be used to save custom files related to a particular save, and load them later.</br>
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public string GetUniqueString(string saveFilePath)
        {
            // Hash the code
            return ComputeHash(saveFilePath);
        }

        /// <summary>
        /// Returns a path to a folder where you can store all your data.
        /// <br>If the folder does not yet exist, it will create it for you.</br>
        /// <br>If you provided a fileName argument, it will Path.Combine it for you.</br>
        /// </summary>
        /// <param name="executingAssembly"></param>
        /// <param name="fileName">(optional)</param>
        /// <returns></returns>
        public string GetSavestoreDirectoryPath(Assembly executingAssembly, string fileName = null)
        {
            var path = Path.Combine(Path.GetDirectoryName(executingAssembly.Location), "Savestore");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            if (!string.IsNullOrWhiteSpace(fileName))
                path = Path.Combine(path, fileName);
            return path;
        }

        // FNV prime and offset basis for 32-bit hash
        private const uint FnvPrime = 16777619;
        private const uint FnvOffsetBasis = 2166136261;

        /// <summary>
        /// Simple FNV-1a hashing algorithm.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        private static string ComputeHash(string input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            uint hash = FnvOffsetBasis;
            foreach (char c in input)
            {
                hash ^= c;
                hash *= FnvPrime;
            }
            return hash.ToString();
        }

        internal void OnSave(string path, bool after)
        {
            if (after)
                OnAfterSave?.Invoke(this, new SaveGameArgs(path));
            else
                OnBeforeSave?.Invoke(this, new SaveGameArgs(path));

            // Handle sync disk data install/upgrade
            if (after)
            {
                Lib.SyncDisks.CheckForSyncDiskData(false, path);
                Lib.PlayerStatus.Save(path);
            }
        }

        internal void OnLoad(string path, bool after)
        {
            if (after)
                OnAfterLoad?.Invoke(this, new SaveGameArgs(path));
            else
                OnBeforeLoad?.Invoke(this, new SaveGameArgs(path));

            // Handle sync disk data install/upgrade
            if (after)
            {
                Lib.SyncDisks.CheckForSyncDiskData(true, path);
                Lib.PlayerStatus.Load(path);
            }
        }

        internal void OnDelete(string path, bool after)
        {
            if (after)
                OnAfterDelete?.Invoke(this, new SaveGameArgs(path));
            else
                OnBeforeDelete?.Invoke(this, new SaveGameArgs(path));

            // Delete sync disk data for this save
            var hash = Lib.SaveGame.GetUniqueString(path);
            var syncDiskDataPath = Lib.SaveGame.GetSavestoreDirectoryPath(Assembly.GetExecutingAssembly(), $"syncdiskdata_{hash}.json");
            if (File.Exists(syncDiskDataPath))
                File.Delete(syncDiskDataPath);
        }

        internal void OnNewGame(bool after)
        {
            if (after)
                OnAfterNewGame?.Invoke(this, EventArgs.Empty);
            else
            {
                Lib.PlayerStatus.ResetStatusTracking();
                OnBeforeNewGame?.Invoke(this, EventArgs.Empty);
            }

            // Clear installed sync disks
            Lib.SyncDisks.InstalledSyncDisks.Clear();
        }
    }

    public sealed class SaveGameArgs : EventArgs
    {
        public string FilePath { get; }

        internal SaveGameArgs(string filePath)
        {
            FilePath = filePath;
        }
    }
}
