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

        internal void OnSave(string path, bool after)
        {
            if (after)
                OnAfterSave?.Invoke(this, new SaveGameArgs(path));
            else
                OnBeforeSave?.Invoke(this, new SaveGameArgs(path));
        }

        internal void OnLoad(string path, bool after)
        {
            if (after)
                OnAfterLoad?.Invoke(this, new SaveGameArgs(path));
            else
                OnBeforeLoad?.Invoke(this, new SaveGameArgs(path));
        }

        internal void OnDelete(string path, bool after)
        {
            if (after)
                OnAfterDelete?.Invoke(this, new SaveGameArgs(path));
            else
                OnBeforeDelete?.Invoke(this, new SaveGameArgs(path));
        }
    }

    public sealed class SaveGameArgs : EventArgs
    {
        public string FilePath { get; }
        
        public SaveGameArgs(string filePath)
        {
            FilePath = filePath;
        }
    }
}
