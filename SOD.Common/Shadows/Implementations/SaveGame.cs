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

        /// <summary>
        /// Returns a unique hashed string related to the savegame. 
        /// <br>Will always return the same unique code for the same <see cref="StateSaveData"/>.</br>
        /// <br>Can be used to save custom files related to a particular save, and load them later.</br>
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public string GetUniqueString(StateSaveData data)
        {
            // TODO: Check if we can use "RestartSafeController.Instance.saveStateFileInfo".
            // These properties should be unique enough per save
            var gameTime = data.gameTime;
            var gameLength = data.gameLength;
            var playerPos = data.playerPos;
            var playerRot = data.playerRot;
            var cityShare = data.cityShare;

            // Build a unique hash based on above data
            float uniqueNumber = gameTime + gameLength + playerPos.x + playerPos.y + playerPos.z + playerRot.x + playerRot.y + playerRot.z;

            // Combination
            var combinedCode = cityShare + "." + uniqueNumber;

            // Hash the code
            return GetSHA256Hash(combinedCode);
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

        internal void GameSaved(StateSaveData stateSaveData, bool after)
        {
            if (after)
                OnAfterGameSave?.Invoke(this, new SaveGameArgs(stateSaveData));
            else
                OnBeforeGameSave?.Invoke(this, new SaveGameArgs(stateSaveData));
        }

        internal void GameLoaded(StateSaveData stateSaveData, bool after)
        {
            if (after)
                OnAfterGameLoad?.Invoke(this, new SaveGameArgs(stateSaveData));
            else
                OnBeforeGameLoad?.Invoke(this, new SaveGameArgs(stateSaveData));
        }
    }

    public sealed class SaveGameArgs : EventArgs
    {
        private readonly StateSaveData _stateSaveData;
        public StateSaveData StateSaveData
        {
            get
            {
                if (_stateSaveData == null)
                    Plugin.Log.LogWarning("StateSaveData is unavailable in \"OnGameSaveBefore\" event, use the \"OnGameSaveBefore\" event if you require this data.");
                return _stateSaveData;
            }
        }

        public SaveGameArgs(StateSaveData stateSaveData)
        {
            _stateSaveData = stateSaveData;
        }
    }
}
