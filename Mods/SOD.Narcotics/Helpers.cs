namespace SOD.Narcotics
{
    internal static class Helpers
    {
        internal static float ApplyPercentageChange(float initial, float percentage, bool increment)
        {
            var change = initial / 100f * percentage;
            return increment ? initial + change : initial - change;
        }
    }
}
