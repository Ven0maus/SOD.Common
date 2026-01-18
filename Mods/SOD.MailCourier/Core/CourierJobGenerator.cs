using SOD.Common.Custom;
using SOD.Common.Extensions;
using SOD.Common.Helpers;
using SOD.CourierJobs;
using SOD.CourierJobs.Core;
using System;
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

        private static readonly Dictionary<int, CourierJob> _courierJobsBySealedMail = new();
        private static readonly Dictionary<int, List<CourierJob>> _courierJobsByMailbox = new();
        private static Dictionary<int, Interactable> _mailboxLocations;
        private static List<Interactable> _mailboxList;

        private static readonly Lazy<JsonSerializerOptions> _serializerOptions = new(() =>
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            options.Converters.Add(new Vector3JsonConverter());
            options.Converters.Add(new TimeDataJsonConverter());
            return options;
        });

        /// <summary>
        /// The interaction action with its AIActionPreset
        /// </summary>
        internal static readonly FirstPersonItem.FPSInteractionAction InsertMailInteractionAction = new()
        {
            action = new AIActionPreset { presetName = "mail_courier_job_message", onlyAvailableWhenItemSelected = true },
            interactionName = "mail_courier_job_message",
            keyOverride = InteractablePreset.InteractionKey.primary
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
            var courierJob = new CourierJob(mailbox.id, sealedMailId, fullAddress);

            // Add to by sealed mail
            _courierJobsBySealedMail.Add(sealedMailId, courierJob);

            // Add to by mailbox
            if (!_courierJobsByMailbox.TryGetValue(mailbox.id, out var jobs))
                _courierJobsByMailbox[mailbox.id] = jobs = new List<CourierJob>();
            jobs.Add(courierJob);

            // Set waypoint to address if no waypoint is currently set
            HandleRoutePlotting(address);
        }

        private static void HandleRoutePlotting(NewAddress address)
        {
            if (MapController.Instance.playerRoute != null)
            {
                if (Plugin.Instance.Config.OverwriteExistingWaypoint)
                {
                    MapController.Instance.playerRoute.Remove();
                    MapController.Instance.PlotPlayerRoute(address);
                }
            }
            else
            {
                MapController.Instance.PlotPlayerRoute(address);
            }
        }

        /// <summary>
        /// Removes any courier jobs that are invalid or outdated.
        /// </summary>
        internal static void CleanupCourierJobs()
        {
            if (_courierJobsBySealedMail.Count == 0) return;

            var currentTime = Common.Lib.Time.CurrentDateTime;
            foreach (var courierJob in _courierJobsBySealedMail.Values.ToList())
            {
                var mail = FindSealedMail(courierJob.SealedMailId);
                if (mail == null || mail.rem)
                {
                    DestroyCourierJob(courierJob);
                    continue;
                }

                // Check if mail item is in the inventory of the player, then update last time active
                if (Common.Lib.Gameplay.HasInteractableInInventory("DeliverableMailItemPreset", out Interactable[] mails))
                {
                    if (mails.Any(a => a.id == courierJob.SealedMailId))
                        courierJob.LastTimeActive = currentTime;
                }

                // When mail is outside of inventory for too long, clean it up
                if (courierJob.LastTimeActive.AddHours(4) <= currentTime)
                    DestroyCourierJob(courierJob);
            }
        }

        /// <summary>
        /// This completely removes a courier job and its mail item.
        /// </summary>
        internal static void DestroyCourierJob(CourierJob courierJob)
        {
            if (_courierJobsBySealedMail.ContainsKey(courierJob.SealedMailId))
            {
                var mailItem = FindSealedMail(courierJob.SealedMailId);
                if (mailItem != null && mailItem.id > 0 && !mailItem.rem)
                    mailItem.Delete();

                // Remove from by sealed mail
                _courierJobsBySealedMail.Remove(courierJob.SealedMailId);

                // Remove from by mailbox
                var jobs = _courierJobsByMailbox[courierJob.MailboxId];
                jobs.Remove(courierJob);
                if (jobs.Count == 0)
                    _courierJobsByMailbox.Remove(courierJob.MailboxId);
            }
        }

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
        internal static CourierJob FindJobBySealedMailId(int sealedMailId)
        {
            return _courierJobsBySealedMail.TryGetValue(sealedMailId, out var courierJob) ? courierJob : null;
        }

        /// <summary>
        /// Find all courier jobs for a specified mailbox id.
        /// </summary>
        /// <param name="mailboxId"></param>
        /// <returns></returns>
        internal static IReadOnlyList<CourierJob> FindJobsByMailboxId(int mailboxId)
        {
            return _courierJobsByMailbox.TryGetValue(mailboxId, out var courierJobs) ?
                courierJobs : new List<CourierJob>();
        }

        /// <summary>
        /// Loads the courier jobs from the mod savestore directory.
        /// </summary>
        /// <param name="saveGameArgs"></param>
        internal static void LoadFromFile(SaveGameArgs saveGameArgs)
        {
            _courierJobsBySealedMail.Clear();
            _courierJobsByMailbox.Clear();

            var saveGameDataPath = Common.Lib.SaveGame.GetSaveGameDataPath(saveGameArgs);
            var filePath = Path.Combine(saveGameDataPath, SAVEDATA_FILENAME);
            if (!File.Exists(filePath)) return;

            var courierJobs = JsonSerializer.Deserialize<List<CourierJob>>(File.ReadAllText(filePath), _serializerOptions.Value);
            foreach (var courierJob in courierJobs)
            {
                // Add to sealed mail jobs
                _courierJobsBySealedMail.Add(courierJob.SealedMailId, courierJob);

                // Add to mailbox jobs
                if (!_courierJobsByMailbox.TryGetValue(courierJob.MailboxId, out var courierJobsMailbox))
                    _courierJobsByMailbox[courierJob.MailboxId] = courierJobsMailbox = new List<CourierJob>();
                courierJobsMailbox.Add(courierJob);
            }

            Plugin.Log.LogInfo($"Loaded {courierJobs.Count} active player mail courier jobs.");
        }

        /// <summary>
        /// Saves the courier jobs to the mod savestore directory.
        /// </summary>
        /// <param name="saveGameArgs"></param>
        internal static void SaveToFile(SaveGameArgs saveGameArgs)
        {
            var saveGameDataPath = Common.Lib.SaveGame.GetSaveGameDataPath(saveGameArgs);
            var filePath = Path.Combine(saveGameDataPath, SAVEDATA_FILENAME);

            if (_courierJobsBySealedMail.Count == 0)
            {
                // Cleanup any files if previous save had courier jobs
                if (File.Exists(filePath))
                    File.Delete(filePath);
                return;
            }

            var json = JsonSerializer.Serialize(_courierJobsBySealedMail.Values.ToList(), _serializerOptions.Value);
            File.WriteAllText(filePath, json);

            Plugin.Log.LogInfo($"Saved {_courierJobsBySealedMail.Count} active player mail courier jobs.");
        }

        /// <summary>
        /// Registers the mail courier job in the newsstands.
        /// </summary>
        internal static void RegisterMailCourierJobs()
        {
            // Make sure to clear everything, any previous states
            _courierJobsBySealedMail.Clear();
            _courierJobsByMailbox.Clear();
            _mailboxLocations = null;
            _mailboxList = null;

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
            Common.Lib.DdsStrings["evidence.names", copy.presetName] = "Sealed Mail"; // What the actual item is named when in the world
            Common.Lib.DdsStrings["evidence.names", copy.name] = "Mail Courier Job"; // What the item is named in the news stand
            Common.Lib.DdsStrings["ui.interaction", "mail_courier_job_message"] = "Insert"; // New interaction to insert into mailbox

            // Setup interactable preset
            copy.placeAtHome = false;
            copy.spawnEvidence = null;
            copy.useEvidence = false;
            copy.findEvidence = InteractablePreset.FindEvidence.none;
            copy.readingSource = InteractablePreset.ReadingModeSource.evidenceNote;
            copy.value = new Vector2(50, 50);
            copy.retailItem = copyRetail;
            copy.summaryMessageSource = "mail_courier_job_message";

            // Insert our custom action
            InsertMailInteractionAction.action.availableWhenItemsSelected.Add(copy.fpsItem);
            copy.fpsItem.actions.Add(InsertMailInteractionAction);

            // Setup retail item preset
            copyRetail.itemPreset = copy;
            copyRetail.menuCategory = RetailItemPreset.MenuCategory.none;
            copyRetail.desireCategory = CompanyPreset.CompanyCategory.retail;
            copyRetail.isConsumable = false;
            copyRetail.canBeFavourite = false;
            copyRetail.isHot = false;

            // Add to game's presets
            GameExtensions.GetResourceCacheCollection<RetailItemPreset>(Toolbox.Instance)[copyRetail.presetName] = copyRetail;
            Toolbox.Instance.objectPresetDictionary[copy.presetName] = copy;

            // Add to the top of all newsstands in the game
            foreach (var newsStandMenu in newsStandMenus)
            {
                var previousItem = newsStandMenu.itemsSold
                    .Where(a => a.presetName == "DeliverableMailItemPreset")
                    .FirstOrDefault();
                if (previousItem != null)
                {
                    newsStandMenu.itemsSold.Remove(previousItem);
                    newsStandMenu.itemsSold.Insert(0, copy);
                }
                else
                {
                    newsStandMenu.itemsSold.Insert(0, copy);
                }
            }

            Plugin.Log.LogInfo("Registered mail courier job in newsstands.");
        }
    }
}
