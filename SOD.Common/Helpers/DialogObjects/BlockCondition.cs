namespace SOD.Common.Helpers.DialogObjects
{
    public sealed class BlockCondition : DDSSaveClasses.DDSBlockCondition
    {
        internal BlockCondition() { }

        // Hide some properties exposed from the game
        internal new string blockID;
        internal new string instanceID;
    }
}
