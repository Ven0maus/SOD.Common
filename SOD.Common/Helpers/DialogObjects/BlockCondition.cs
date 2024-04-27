namespace SOD.Common.Helpers.DialogObjects
{
    public sealed class BlockCondition : DDSSaveClasses.DDSBlockCondition
    {
        internal BlockCondition() { }

        // Hide some properties exposed from the game
#pragma warning disable CS0649
        internal new string blockID;
        internal new string instanceID;
#pragma warning restore CS0649
    }
}
