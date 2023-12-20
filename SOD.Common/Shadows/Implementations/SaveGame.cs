using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace SOD.Common.Shadows.Implementations
{
    public sealed class SaveGame
    {
        /// <summary>
        /// Raised right before a savegame is saved.
        /// <br>Note: StateSaveData is unavailable in the before event.</br>
        /// </summary>
        public event EventHandler<SaveGameArgs> OnBeforeGameSave;
        /// <summary>
        /// Raised right after a savegame is saved.
        /// </summary>
        public event EventHandler<SaveGameArgs> OnAfterGameSave;
        /// <summary>
        /// Raised right before a savegame is loaded.
        /// </summary>
        public event EventHandler<SaveGameArgs> OnBeforeGameLoad;
        /// <summary>
        /// Raised right after a savegame is loaded.
        /// </summary>
        public event EventHandler<SaveGameArgs> OnAfterGameLoad;

        // TODO: Add events for SaveFileDeletion

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
            return GetSHA256Hash(saveFilePath);
        }
        
        /// <summary>
        /// Returns a path to a folder where you can store all your data.
        /// <br>If the folder does not yet exist, it will create it for you.</br>
        /// </summary>
        /// <returns></returns>
        public string GetSavestoreDirectoryPath(Assembly executingAssembly)
        {
            var path = Path.Combine(Path.GetDirectoryName(executingAssembly.Location), "Savestore");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }

        private static string GetSHA256Hash(string input)
        {
            using SHA256 sha256 = SHA256.Create();
            byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Convert the byte array to a hexadecimal string
            StringBuilder hashBuilder = new();
            foreach (byte b in hashBytes)
            {
                hashBuilder.Append(b.ToString("x2"));
            }

            return hashBuilder.ToString();
        }

        internal void GameSaved(string path, bool after)
        {
            if (after)
                OnAfterGameSave?.Invoke(this, new SaveGameArgs(path));
            else
                OnBeforeGameSave?.Invoke(this, new SaveGameArgs(path));
        }

        internal void GameLoaded(string path, bool after)
        {
            if (after)
                OnAfterGameLoad?.Invoke(this, new SaveGameArgs(path));
            else
                OnBeforeGameLoad?.Invoke(this, new SaveGameArgs(path));
        }
    }

    public sealed class SaveGameArgs : EventArgs
    {
        private readonly string _filePath;
        public string FilePath
        {
            get
            {
                if (_filePath == null)
                    Plugin.Log.LogWarning("Current is unavailable in \"OnGameSaveBefore\" event, use the \"OnGameSaveBefore\" event if you require this data.");
                return _filePath;
            }
        }
        
        public SaveGameArgs(string filePath)
        {
            _filePath = filePath;
        }
    }
}
