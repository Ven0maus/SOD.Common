using SOD.Common;
using SOD.Common.Extensions;
using SOD.Common.Helpers;
using SOD.CourierJobs.Core;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using UnityEngine;

namespace SOD.MailCourier.Core
{
    internal static class CourierJobGenerator
    {
        private const string SAVEDATA_FILENAME = "mailcourier_savedata.json";

        private static readonly Dictionary<int, CourierJob> _courierJobs = new();

        private static readonly JsonSerializerOptions _serializerOptions = new()
        {
            WriteIndented = true
        };

        /// <summary>
        /// Generate a new courier job for the player.
        /// </summary>
        /// <param name="courierJob"></param>
        /// <returns></returns>
        internal static void CreateJob(int sealedMailId)
        {
            // Find a random mailbox in the world
            var mailbox = FindMailbox(null);
            if (mailbox == null)
            {
                Plugin.Log.LogError("Unable to create mail courier job, no mailboxes found in the world.");
                return;
            }

            // Store the full address so we don't need to look it up each time
            var address = mailbox.objectRef.TryCast<NewAddress>();
            var fullAddress = address.residence.GetResidenceString() + " " + address.building.name;

            _courierJobs.Add(sealedMailId, new CourierJob(mailbox.id, sealedMailId, fullAddress));
        }

        /// <summary>
        /// This completely removes a courier job and its mail item.
        /// </summary>
        internal static void DestroyCourierJob(CourierJob courierJob)
        {
            if (_courierJobs.ContainsKey(courierJob.SealedMailId))
            {
                var mailItem = FindSealedMail(courierJob.SealedMailId);
                if (mailItem != null && mailItem.id > 0 && !mailItem.rem)
                    mailItem.Delete();
                _courierJobs.Remove(courierJob.SealedMailId);
            }
        }

        private static Dictionary<int, Interactable> _mailboxLocations;
        private static List<Interactable> _mailboxList;

        /// <summary>
        /// Finds a mailbox by id, or returns a random one from the pool if no id is specified.
        /// </summary>
        /// <param name="mailboxId"></param>
        /// <returns></returns>
        internal static Interactable FindMailbox(int? mailboxId)
        {
            if (_mailboxLocations == null)
            {
                var groups = CityData.Instance.residenceDirectory
                    .Where(a => a.mailbox != null && a.mailbox.integratedInteractables != null && a.mailbox.integratedInteractables.Count > 0)
                    .GroupBy(a => a.mailbox.id);

                _mailboxList = new List<Interactable>();
                foreach (var group in groups)
                {
                    var firstEntry = group.First();
                    _mailboxList.AddRange(firstEntry.mailbox.integratedInteractables.Where(a =>
                    {
                        if (a.objectRef == null) return false;
                        var address = a.objectRef.TryCast<NewAddress>();
                        if (address == null) return false;
                        return address.residence != null;
                    }));
                }

                if (Plugin.Instance.Config.DebugMode)
                    Plugin.Log.LogInfo("[DEBUG]: Found a total of " + _mailboxList.Count + " mailboxes.");

                _mailboxLocations = _mailboxList.ToDictionary(a => a.id);
            }

            if (mailboxId.HasValue)
                return _mailboxLocations[mailboxId.Value];

            // Return a random valid mailbox
            return _mailboxList[System.Random.Shared.Next(_mailboxList.Count)];
        }

        /// <summary>
        /// Finds the sealed mail item by id.
        /// </summary>
        /// <param name="sealedMailId"></param>
        /// <returns></returns>
        internal static Interactable FindSealedMail(int sealedMailId)
        {
            return CityData.Instance.savableInteractableDictionary
                .TryGetValue(sealedMailId, out var mailItem) ?
                    mailItem : null;
        }

        /// <summary>
        /// Returns the courier job for the specified sealed mail id.
        /// </summary>
        /// <param name="sealedMailId"></param>
        /// <returns></returns>
        internal static CourierJob FindJob(int sealedMailId)
        {
            return _courierJobs.TryGetValue(sealedMailId, out var courierJob) ? courierJob : null;
        }

        /// <summary>
        /// Loads the courier jobs from the mod savestore directory.
        /// </summary>
        /// <param name="saveGameArgs"></param>
        internal static void LoadFromFile(SaveGameArgs saveGameArgs)
        {
            _courierJobs.Clear();

            var saveGameDataPath = Lib.SaveGame.GetSaveGameDataPath(saveGameArgs);
            var filePath = Path.Combine(saveGameDataPath, SAVEDATA_FILENAME);
            if (!File.Exists(filePath)) return;

            var courierJobs = JsonSerializer.Deserialize<List<CourierJob>>(filePath, _serializerOptions);
            foreach (var courierJob in courierJobs)
                _courierJobs.Add(courierJob.SealedMailId, courierJob);

            Plugin.Log.LogInfo($"Loaded {courierJobs.Count} active player mail courier jobs.");
        }

        /// <summary>
        /// Saves the courier jobs to the mod savestore directory.
        /// </summary>
        /// <param name="saveGameArgs"></param>
        internal static void SaveToFile(SaveGameArgs saveGameArgs)
        {
            var saveGameDataPath = Lib.SaveGame.GetSaveGameDataPath(saveGameArgs);
            var filePath = Path.Combine(saveGameDataPath, SAVEDATA_FILENAME);

            if (_courierJobs.Count == 0)
            {
                // Cleanup any files if previous save had courier jobs
                if (File.Exists(filePath))
                    File.Delete(filePath);
                return;
            }

            var json = JsonSerializer.Serialize(_courierJobs.Values.ToList(), _serializerOptions);
            File.WriteAllText(filePath, json);

            Plugin.Log.LogInfo($"Saved {_courierJobs.Count} active player mail courier jobs.");
        }

        /// <summary>
        /// Registers the mail courier job in the newsstands.
        /// </summary>
        internal static void RegisterMailCourierJobs()
        {
            var newsStand = Common.Helpers.SyncDiskObjects.SyncDiskBuilder.SyncDiskSaleLocation.Newsstand.ToString();
            var newsStandMenus = Toolbox.Instance.GetFromResourceCache<MenuPreset>()
                .Where(a => a.GetPresetName() == newsStand);

            // Create new interactable preset (similar to secret note)
            var sealedEnvelope = Toolbox.Instance.GetInteractablePreset("SealedEnvelope");
            var cleaningSprayRetail = Toolbox.Instance.GetFromResourceCache<RetailItemPreset>("CleaningSpray");
            var copy = ScriptableObject.Instantiate(sealedEnvelope);
            var copyRetail = ScriptableObject.Instantiate(cleaningSprayRetail);

            // Set names
            copy.presetName = "DeliverableMailItemPreset";
            copy.name = "DeliverableMailItemPreset_ShopText";
            copyRetail.presetName = "DeliverableMailItemRetailPreset";
            copyRetail.name = copyRetail.presetName;

            // Register new name for it in DDS strings
            Lib.DdsStrings["evidence.names", copy.presetName] = "Sealed Mail"; // What the actual item is named when in the world
            Lib.DdsStrings["evidence.names", copy.name] = "Mail Courier Job"; // What the item is named in the news stand

            // Setup interactable preset
            copy.placeAtHome = false;
            copy.spawnEvidence = null;
            copy.useEvidence = false;
            copy.findEvidence = InteractablePreset.FindEvidence.none;
            copy.readingSource = InteractablePreset.ReadingModeSource.evidenceNote;
            copy.value = new Vector2(50, 50);
            copy.retailItem = copyRetail;
            copy.summaryMessageSource = "mail_courier_job_message";

            // Setup retail item preset
            copyRetail.itemPreset = copy;
            copyRetail.menuCategory = RetailItemPreset.MenuCategory.none;
            copyRetail.desireCategory = CompanyPreset.CompanyCategory.retail;
            copyRetail.isConsumable = false;
            copyRetail.canBeFavourite = false;
            copyRetail.isHot = false;

            // Add to game's presets
            GameExtensions.GetResourceCacheCollection<RetailItemPreset>(Toolbox.Instance).Add(copyRetail.presetName, copyRetail);
            Toolbox.Instance.objectPresetDictionary.Add(copy.presetName, copy);

            // Add to the top of all newsstands in the game
            foreach (var newsStandMenu in newsStandMenus)
                newsStandMenu.itemsSold.Insert(0, copy);

            Plugin.Log.LogInfo("Registered mail courier job in newsstands.");
        }
    }
}
