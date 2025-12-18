using SOD.Common;
using SOD.Common.Helpers;
using SOD.QoL.Objects;
using System;
using System.IO;
using System.Reflection;
using System.Text.Json;

namespace SOD.QoL.Patches
{
    internal class SideJobPatches
    {
        private const string _oldExpirationSaveFile = "SideJobSaveData_{0}.json";
        private const string _newExpirationSaveFile = "sod_qol_SideJobSaveData.json";
        private static ExpirationSaveData _expirationSaveData;

        internal static void ExpireTimedOutJobs()
        {
            var controller = SideJobController.Instance;
            if (!Lib.Time.IsInitialized || controller == null || _expirationSaveData == null) return;

            var currentTime = Lib.Time.CurrentDateTime;
            for (int i = 0; i < controller.jobTracking.Count; i++)
            {
                SideJobController.JobTracking jobTracking = controller.jobTracking[i];
                for (int j = 0; j < jobTracking.activeJobs.Count; j++)
                {
                    SideJob job = jobTracking.activeJobs[j];

                    // If it exists we need to check if the job is accepted in the meantime
                    if (_expirationSaveData.Expirations.ContainsKey(job.jobID.ToString()))
                    {
                        if (job.accepted)
                        {
                            // This job is accepted, we no longer need to track the expiration.
                            _expirationSaveData.Expirations.Remove(job.jobID.ToString());
                            continue;
                        }
                    }

                    // If the job is not yet accepted and still in posted state then check if its expired or not
                    if (!job.accepted && job.state == SideJob.JobState.posted)
                    {
                        if (_expirationSaveData.Expirations.TryGetValue(job.jobID.ToString(), out var jobExpireTime))
                        {
                            if (jobExpireTime <= currentTime)
                            {
                                // Job is expired, we end it
                                job.End();

                                // Remove also from tracking
                                _expirationSaveData.Expirations.Remove(job.jobID.ToString());
                            }
                        }
                        else
                        {
                            // If it doesn't exist yet we'll just add it here
                            var expireHours = Plugin.Instance.Config.RandomizeExpireTime ?
                                Plugin.Random.Next(Plugin.Instance.Config.ExpireTimeMin, Plugin.Instance.Config.ExpireTimeMax + 1) :
                                Plugin.Instance.Config.ExpireTimeMax;
                            _expirationSaveData.Expirations[job.jobID.ToString()] = Lib.Time.CurrentDateTime.AddHours(expireHours);
                        }
                    }
                }
            }
        }

        internal static void InitializeExpireTimes(SaveGameArgs saveGameArgs)
        {
            var filePath = new Lazy<string>(() =>
            {
                var fileName = string.Format(_oldExpirationSaveFile, Lib.SaveGame.GetUniqueString(saveGameArgs.FilePath));
#pragma warning disable CS0618 // Type or member is obsolete
                // Support outdated format
                var oldSavePath = Lib.SaveGame.GetSavestoreDirectoryPath(Assembly.GetExecutingAssembly(), fileName);
#pragma warning restore CS0618 // Type or member is obsolete
                return Lib.SaveGame.MigrateOldSaveStructure(oldSavePath, saveGameArgs, _newExpirationSaveFile);
            });

            if (saveGameArgs == null || !File.Exists(filePath.Value))
            {
                _expirationSaveData = new()
                {
                    Expirations = new()
                };
                return;
            }

            try
            {
                var options = new JsonSerializerOptions
                {
                    Converters = { new TimeDataJsonConverter() },
                    WriteIndented = true
                };

                var json = File.ReadAllText(filePath.Value);
                _expirationSaveData = ExpirationSaveData.Deserialize(json);
                Plugin.Log.LogInfo("Loaded SideJobSaveData from file.");
            }
            catch (Exception e)
            {
                Plugin.Log.LogError($"Unable to read SideJobSaveData file (corrupted?): {e.Message}");
                _expirationSaveData = new()
                {
                    Expirations = new()
                };
            }
        }

        internal static void SaveExpireTimes(SaveGameArgs saveGameArgs)
        {
            if (_expirationSaveData != null && _expirationSaveData.Expirations.Count > 0)
            {
                var fileName = string.Format(_oldExpirationSaveFile, Lib.SaveGame.GetUniqueString(saveGameArgs.FilePath));
#pragma warning disable CS0618 // Type or member is obsolete
                // Support outdated format
                var oldFilePath = Lib.SaveGame.GetSavestoreDirectoryPath(Assembly.GetExecutingAssembly(), fileName);
#pragma warning restore CS0618 // Type or member is obsolete
                var newFilePath = Lib.SaveGame.MigrateOldSaveStructure(oldFilePath, saveGameArgs, _newExpirationSaveFile);

                try
                {
                    var json = _expirationSaveData.Serialize();
                    File.WriteAllText(newFilePath, json);
                    Plugin.Log.LogInfo("Saved SideJobSaveData to file.");
                }
                catch (Exception e)
                {
                    Plugin.Log.LogError($"Unable to save side job expiration timers to file: {e.Message}");
                }
            }
        }

        internal static void DeleteSaveData(SaveGameArgs saveGameArgs)
        {
            // Still support migration, but effective deletion is handled by sod.common
            var fileName = string.Format(_oldExpirationSaveFile, Lib.SaveGame.GetUniqueString(saveGameArgs.FilePath));
#pragma warning disable CS0618 // Type or member is obsolete
            // Support outdated format
            var oldFilePath = Lib.SaveGame.GetSavestoreDirectoryPath(Assembly.GetExecutingAssembly(), fileName);
#pragma warning restore CS0618 // Type or member is obsolete
            _ = Lib.SaveGame.MigrateOldSaveStructure(oldFilePath, saveGameArgs, _newExpirationSaveFile);
        }
    }
}
