using SOD.MailCourier.Core;
using System;
using System.Text.Json.Serialization;
using UnityEngine;

namespace SOD.CourierJobs.Core
{
    /// <summary>
    /// The container of the courier job data, completely serializable.
    /// </summary>
    public sealed class CourierJob
    {
        /// <summary>
        /// The address for the delivery.
        /// </summary>
        public int MailboxId { get; set; }

        /// <summary>
        /// The unique sealed mail item id.
        /// </summary>
        public int SealedMailId { get; set; }

        /// <summary>
        /// The location from where the courier job was acquired. (news stand position)
        /// </summary>
        public Vector3 PointOfOrigin { get; set; }

        /// <summary>
        /// The full delivery address in text format.
        /// </summary>
        public string DeliveryAddressName { get; set; }

        /// <summary>
        /// The payment for this courier job.
        /// </summary>
        [JsonIgnore]
        internal int Payment
        {
            get
            {
                return (int)Math.Round(Plugin.Instance.Config.InitialCost + Plugin.Instance.Config.BaseReward +
                    ((Mailbox.GetWorldPosition() - PointOfOrigin).magnitude * Plugin.Instance.Config.DistanceBonusPercentage));
            }
        }

        /// <summary>
        /// The mailbox for the courier job.
        /// </summary>
        [JsonIgnore]
        internal Interactable Mailbox => CourierJobGenerator.FindMailbox(MailboxId);

        /// <summary>
        /// The interactable for the courier job.
        /// </summary>
        [JsonIgnore]
        internal Interactable MailItem => CourierJobGenerator.FindSealedMail(SealedMailId);

        // For serialization
        public CourierJob() { }

        public CourierJob(int mailboxId, int mailItemId, string deliveryAddressName)
        {
            MailboxId = mailboxId;
            SealedMailId = mailItemId;
            DeliveryAddressName = deliveryAddressName;
        }
    }
}
