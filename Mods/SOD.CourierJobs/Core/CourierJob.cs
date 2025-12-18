namespace SOD.CourierJobs.Core
{
    public class CourierJob
    {
        public CourierJobType Type { get; }
        public Citizen Recipient { get; }
        public Interactable Deliverable { get; }
        public int Price { get; }

        public CourierJob(CourierJobType type, Citizen recipient)
        {
            Type = type;
            Recipient = recipient;
        }
    }

    public enum CourierJobType
    {
        Mail,
        Groceries,
        SignedLetter
    }
}
