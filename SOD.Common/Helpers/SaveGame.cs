using System;
using System.IO;
using System.Reflection;
using UnityEngine;

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

        // FNV prime and offset basis for 32-bit hash
        private const uint FnvPrime = 16777619;
        private const uint FnvOffsetBasis = 2166136261;

        /// <summary>
        /// Creates a unique hash from a string value that is always the same.
        /// <br>Internally it uses FNV hashing.</br>
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public string GetUniqueString(string value)
        {
            return GetFnvHash(value).ToString();
        }

        /// <summary>
        /// Same functionality as <see cref="GetUniqueString(string)"/> but in uint format.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public uint GetUniqueNumber(string value)
        {
            return GetFnvHash(value);
        }

        private static uint GetFnvHash(string value)
        {
            // Hash the value
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            uint hash = FnvOffsetBasis;
            foreach (char c in value)
            {
                hash ^= c;
                hash *= FnvPrime;
            }
            return hash;
        }

        /// <summary>
        /// Use this method to migrate from the old save file data to the new savegame data filepath.
        /// <br>See example usage:</br>
        /// <example>
        /// <code>
        /// var hash = Lib.SaveGame.GetUniqueString(saveGameArgs.FilePath);
        /// var oldFilePath = Lib.SaveGame.GetSavestoreDirectoryPath(
        ///     Assembly.GetExecutingAssembly(),
        ///     $"yourdata_{hash}.json");
        ///
        /// // Handles folder creation, file moving, and cleanup
        /// var newPath = Lib.SaveGame.MigrateOldSaveStructure(
        ///     oldFilePath,
        ///     saveGameArgs,
        ///     "yourdata.json");
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="oldFilePath">The old filepath with the hashed filename</param>
        /// <param name="saveGameArgs">The savegameargs originating from the Lib.SaveGame event</param>
        /// <param name="fileName">The new filename for your data, no longer requires hash appended.</param>
        /// <returns>Returns the new savegame data filepath.</returns>
        public string MigrateOldSaveStructure(string oldFilePath, SaveGameArgs saveGameArgs, string fileName)
        {
            var newPath = GetSaveGameDataPath(saveGameArgs, fileName);
            if (File.Exists(oldFilePath))
                File.Move(oldFilePath, newPath);

            // Delete directory if no files remain
            var dir = Path.GetDirectoryName(oldFilePath);
            var files = Directory.GetFiles(dir, "*", SearchOption.AllDirectories);
            if (files.Length == 0)
                Directory.Delete(dir);

            return newPath;
        }

        /// <summary>
        /// This method is obsolete. (For migration support look into: <see cref="MigrateOldSaveStructure(string, SaveGameArgs, string)"/>).
        /// <br>Instead use <see cref="GetSaveGameDataPath(SaveGameArgs, string)"/> to store savegame-specific data.</br>
        /// <br>Instead use <see cref="GetPluginDataPath(Assembly, string)"/> to store plugin-specific data.</br>
        /// </summary>
        /// <param name="executingAssembly"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        [Obsolete("Use \"GetSaveGameDataPath\" to store savegame-specific data. " +
            "Or use \"GetPluginDataPath\" to store plugin-specific data instead.", false)]
        public string GetSavestoreDirectoryPath(Assembly executingAssembly, string fileName)
        {
            var path = GetSavestoreDirectoryPath(executingAssembly);
            if (!string.IsNullOrWhiteSpace(fileName))
                path = Path.Combine(path, fileName);
            return path;
        }

        /// <summary>
        /// This method is obsolete. (For migration support look into: <see cref="MigrateOldSaveStructure(string, SaveGameArgs, string)"/>).
        /// <br>Instead use <see cref="GetSaveGameDataPath(SaveGameArgs)"/> to store savegame-specific data.</br>
        /// <br>Instead use <see cref="GetPluginDataPath(Assembly)"/> to store plugin-specific data.</br>
        /// </summary>
        /// <param name="executingAssembly"></param>
        /// <returns></returns>
        [Obsolete("Use \"GetSaveGameDataPath\" to store savegame-specific data. " +
            "Or use \"GetPluginDataPath\" to store plugin-specific data instead.", false)]
        public string GetSavestoreDirectoryPath(Assembly executingAssembly)
        {
            var path = Path.Combine(Path.GetDirectoryName(executingAssembly.Location), "Savestore");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }

        /// <summary>
        /// Returns a path to your plugin folder.
        /// <br>Path will be bepinex/plugins/yourpluginname</br>
        /// </summary>
        /// <param name="executingAssembly"></param>
        /// <returns></returns>
        public string GetPluginDirectoryPath(Assembly executingAssembly)
        {
            var path = Path.GetDirectoryName(executingAssembly.Location);
            if (!Directory.Exists(path))
                throw new Exception("There is no plugin directory for this assembly location: " + path);
            return path;
        }

        /// <summary>
        /// Returns a save-game specific folder that you can use to store savegame-specific data.
        /// <br>The SaveGameArgs object can be retrieved from the Lib.SaveGame events.</br>
        /// </summary>
        /// <param name="saveGameArgs"></param>
        /// <returns></returns>
        public string GetSaveGameDataPath(SaveGameArgs saveGameArgs)
        {
            var path = Path.GetDirectoryName(saveGameArgs.FilePath);
            var folderName = Path.GetFileNameWithoutExtension(saveGameArgs.FilePath);
            var saveGameDataPath = Path.Combine(path, folderName + "_mod_savestore");
            if (!Directory.Exists(saveGameDataPath))
                Directory.CreateDirectory(saveGameDataPath);
            return saveGameDataPath;
        }

        /// <summary>
        /// Returns a save-game specific folder that you can use to store savegame-specific data.
        /// <br>The SaveGameArgs object can be retrieved from the Lib.SaveGame events.</br>
        /// </summary>
        /// <param name="saveGameArgs">The saveFilePath provided by savegame event arguments.</param>
        /// <param name="fileName">The filename you wish to store.</param>
        /// <returns></returns>
        public string GetSaveGameDataPath(SaveGameArgs saveGameArgs, string fileName)
        {
            var path = Path.GetDirectoryName(saveGameArgs.FilePath);
            var folderName = Path.GetFileNameWithoutExtension(saveGameArgs.FilePath);
            var saveGameDataPath = Path.Combine(path, folderName + "_mod_savestore");
            if (!Directory.Exists(saveGameDataPath))
                Directory.CreateDirectory(saveGameDataPath);

            var newPath = Path.Combine(saveGameDataPath, fileName);
            path = Path.GetDirectoryName(newPath);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return newPath;
        }

        /// <summary>
        /// Returns a path to a folder to store plugin specific data.
        /// <br>Path will be bepinex/plugins/yourpluginname/PluginData</br>
        /// </summary>
        /// <param name="executingAssembly"></param>
        /// <returns></returns>
        public string GetPluginDataPath(Assembly executingAssembly)
        {
            var path = Path.Combine(GetPluginDirectoryPath(executingAssembly), "PluginData");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }

        /// <summary>
        /// Returns a path to a folder to store plugin specific data.
        /// <br>Path will be bepinex/plugins/yourpluginname/PluginData</br>
        /// </summary>
        /// <param name="executingAssembly"></param>
        /// <returns></returns>
        public string GetPluginDataPath(Assembly executingAssembly, string fileName)
        {
            var path = Path.Combine(GetPluginDirectoryPath(executingAssembly), "PluginData");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return Path.Combine(path, fileName);
        }

        internal bool IsSaving { get; private set; }
        internal void OnSave(string path, bool after)
        {
            var args = new SaveGameArgs(path);
            if (after)
                OnAfterSave?.Invoke(this, args);
            else
                OnBeforeSave?.Invoke(this, args);

            // Handle sync disk data install/upgrade
            if (after)
            {
                IsSaving = false;
                // Reset illegal action timer to max value
                if (Lib.PlayerStatus.IllegalStatusModifierDictionary != null && Lib.PlayerStatus.IllegalStatusModifierDictionary.Count > 0)
                    Player.Instance.illegalActionTimer = float.MaxValue;

                Lib.SyncDisks.CheckForSyncDiskData(false, args);
                Lib.PlayerStatus.Save(args);
            }
            else
            {
                IsSaving = true;
                // Set illegal action timer to low number to prevent saves breaking (incase sod.common is ever uninstalled while its at maxvalue)
                if (Lib.PlayerStatus.IllegalStatusModifierDictionary != null && Lib.PlayerStatus.IllegalStatusModifierDictionary.Count > 0)
                    Player.Instance.illegalActionTimer = 1f;
            }

            // Delete directory if no files remain
            var dir = GetSaveGameDataPath(args);
            var files = Directory.GetFiles(dir, "*", SearchOption.AllDirectories);
            if (files.Length == 0)
                Directory.Delete(dir);
        }

        internal void OnLoad(string path, bool after)
        {
            var args = new SaveGameArgs(path);
            if (after)
                OnAfterLoad?.Invoke(this, args);
            else
                OnBeforeLoad?.Invoke(this, args);

            // Handle sync disk data install/upgrade
            if (after)
            {
                Lib.SyncDisks.CheckForSyncDiskData(true, args);
                Lib.PlayerStatus.Load(args);
            }
            else
            {
                Lib.PlayerStatus.ResetStatusTracking();
            }

            // Delete directory if no files remain
            var dir = GetSaveGameDataPath(args);
            var files = Directory.GetFiles(dir, "*", SearchOption.AllDirectories);
            if (files.Length == 0)
                Directory.Delete(dir);
        }

        internal void OnDelete(string path, bool after)
        {
            var args = new SaveGameArgs(path);
            if (after)
                OnAfterDelete?.Invoke(this, args);
            else
                OnBeforeDelete?.Invoke(this, args);

            // Delete sync disk data for this save
            var hash = Lib.SaveGame.GetUniqueString(path);
#pragma warning disable CS0618 // Type or member is obsolete
            // Needs to remain for API version compatibility
            var oldSyncDiskSavePath = Lib.SaveGame.GetSavestoreDirectoryPath(Assembly.GetExecutingAssembly(), $"syncdiskdata_{hash}.json");
#pragma warning restore CS0618 // Type or member is obsolete
            _ = Lib.SaveGame.MigrateOldSaveStructure(oldSyncDiskSavePath, args, "syncdiskdata.json");

#pragma warning disable CS0618 // Type or member is obsolete
            // Needs to remain for API version compatibility
            var oldPlayerStatusDataPath = Lib.SaveGame.GetSavestoreDirectoryPath(Assembly.GetExecutingAssembly(), $"playerstatus_{hash}.json");
#pragma warning restore CS0618 // Type or member is obsolete
            _ = Lib.SaveGame.MigrateOldSaveStructure(oldPlayerStatusDataPath, args, "playerstatus.json");

            // Delete entire folder and its contents
            Directory.Delete(Lib.SaveGame.GetSaveGameDataPath(args), true);
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
