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
        /// Returns the directory of the plugin assembly.
        /// <para>
        /// Typically: <c>bepinex/plugins/YourPluginName</c>
        /// </para>
        /// </summary>
        /// <param name="executingAssembly">The plugin's executing assembly.</param>
        /// <returns>The absolute path to the plugin directory.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="executingAssembly"/> is null.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the plugin directory cannot be determined.
        /// </exception>
        public string GetPluginDirectoryPath(Assembly executingAssembly)
        {
            if (executingAssembly == null)
                throw new ArgumentNullException(nameof(executingAssembly));

            var location = executingAssembly.Location;

            if (string.IsNullOrWhiteSpace(location))
                throw new InvalidOperationException(
                    "Assembly location is not available. The assembly may be loaded dynamically.");

            var directory = Path.GetDirectoryName(location);

            if (string.IsNullOrWhiteSpace(directory))
                throw new InvalidOperationException(
                    "Could not determine the plugin directory from the assembly location.");

            if (!Directory.Exists(directory))
                throw new DirectoryNotFoundException(
                    $"Plugin directory does not exist: {directory}");

            return directory;
        }

        /// <summary>
        /// Returns a save-game specific folder that you can use to store savegame-specific data.
        /// <br>The SaveGameArgs object can be retrieved from the Lib.SaveGame events.</br>
        /// </summary>
        /// <param name="saveGameArgs"></param>
        /// <returns></returns>
        public string GetSaveGameDataPath(SaveGameArgs saveGameArgs)
        {
            if (saveGameArgs == null)
                throw new ArgumentNullException(nameof(saveGameArgs));

            if (string.IsNullOrWhiteSpace(saveGameArgs.FilePath))
                throw new ArgumentException("Save file path is invalid.", nameof(saveGameArgs));

            var saveDir = Path.GetDirectoryName(saveGameArgs.FilePath)
                ?? throw new InvalidOperationException("Could not determine save directory.");

            var saveName = Path.GetFileNameWithoutExtension(saveGameArgs.FilePath);

            var saveGameDataPath = Path.Combine(
                saveDir,
                saveName + "_mod_savestore");

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
            if (saveGameArgs == null)
                throw new ArgumentNullException(nameof(saveGameArgs));

            if (string.IsNullOrWhiteSpace(saveGameArgs.FilePath))
                throw new ArgumentException("Save file path is invalid.", nameof(saveGameArgs));

            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException("File name must be provided.", nameof(fileName));

            var saveDir = Path.GetDirectoryName(saveGameArgs.FilePath)
                ?? throw new InvalidOperationException("Could not determine save directory.");

            var saveName = Path.GetFileNameWithoutExtension(saveGameArgs.FilePath);

            var basePath = Path.Combine(
                saveDir,
                saveName + "_mod_savestore");

            // Prevent directory traversal
            fileName = fileName.Replace('\\', Path.DirectorySeparatorChar)
                               .Replace('/', Path.DirectorySeparatorChar);

            if (Path.IsPathRooted(fileName))
                throw new ArgumentException("File name must be relative.", nameof(fileName));

            var fullPath = Path.GetFullPath(
                Path.Combine(basePath, fileName));

            // Ensure the final path stays inside basePath
            var fullBasePath = Path.GetFullPath(basePath);
            if (!fullPath.StartsWith(fullBasePath, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Invalid file path.");

            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

            return fullPath;
        }

        /// <summary>
        /// Returns a path to a folder to store plugin specific data.
        /// <br>Path will be bepinex/plugins/yourpluginname/PluginData</br>
        /// </summary>
        /// <param name="executingAssembly"></param>
        /// <returns></returns>
        public string GetPluginDataPath(Assembly executingAssembly)
        {
            if (executingAssembly == null)
                throw new ArgumentNullException(nameof(executingAssembly));

            var assemblyLocation = executingAssembly.Location;
            if (string.IsNullOrWhiteSpace(assemblyLocation))
                throw new InvalidOperationException("Assembly location is not available.");

            var pluginDir = Path.GetDirectoryName(assemblyLocation)
                ?? throw new InvalidOperationException("Could not determine plugin directory.");

            var pluginDataPath = Path.Combine(pluginDir, "PluginData");

            // Idempotent and thread-safe
            Directory.CreateDirectory(pluginDataPath);

            return pluginDataPath;
        }

        /// <summary>
        /// Returns a path to a folder to store plugin specific data.
        /// <br>Path will be bepinex/plugins/yourpluginname/PluginData</br>
        /// </summary>
        /// <param name="executingAssembly"></param>
        /// <returns></returns>
        public string GetPluginDataPath(Assembly executingAssembly, string fileName)
        {
            if (executingAssembly == null)
                throw new ArgumentNullException(nameof(executingAssembly));

            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException("File name must be provided.", nameof(fileName));

            var assemblyLocation = executingAssembly.Location;
            if (string.IsNullOrWhiteSpace(assemblyLocation))
                throw new InvalidOperationException("Assembly location is not available.");

            var pluginDir = Path.GetDirectoryName(assemblyLocation)
                ?? throw new InvalidOperationException("Could not determine plugin directory.");

            var basePath = Path.Combine(pluginDir, "PluginData");

            // Normalize separators
            fileName = fileName
                .Replace('\\', Path.DirectorySeparatorChar)
                .Replace('/', Path.DirectorySeparatorChar);

            // Disallow absolute paths
            if (Path.IsPathRooted(fileName))
                throw new ArgumentException("File name must be a relative path.", nameof(fileName));

            var fullBasePath = Path.GetFullPath(basePath);
            var fullPath = Path.GetFullPath(Path.Combine(fullBasePath, fileName));

            // Prevent directory traversal (../)
            if (!fullPath.StartsWith(fullBasePath, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Invalid file path.");

            // Create directories safely (supports subfolders in fileName)
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

            return fullPath;
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
