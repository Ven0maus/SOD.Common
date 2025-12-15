namespace SOD.CourierJobs.Core
{
    public class CourierJob
    {
        public CourierJobType Type { get; }
        public Citizen Recipient { get; }
        public int Price { get; }
        public Interactable Deliverable { get; }

        private bool _delivered = false;

        public CourierJob(CourierJobType type, Citizen recipient)
        {
            Type = type;
            Recipient = recipient;
        }

        public void Deliver()
        {
            if (_delivered) return;
            _delivered = true;

            // Remove deliverable from the game
            Deliverable.SafeDelete(true);
            Deliverable.Delete(); // To-do check

            // Close the case automatically

            // Payout the money (can be done by case close maybe?)
            GameplayController.Instance.AddMoney(Price, true, "Payment Courier Job");
        }
    }

    public enum CourierJobType
    {
        Mail,
        Groceries,
        SignedLetter
    }
}
