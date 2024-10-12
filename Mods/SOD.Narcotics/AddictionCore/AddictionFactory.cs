using SOD.Narcotics.AddictionCore.Addictions;
using System;

namespace SOD.Narcotics.AddictionCore
{
    public static class AddictionFactory
    {
        public static Addiction Get(AddictionType addictionType)
        {
            switch (addictionType)
            {
                case AddictionType.Alcohol:
                    return new AlcoholAddiction();
                case AddictionType.Opioid:
                    return new OpioidAddiction();
                case AddictionType.Sugar:
                    return new SugarAddiction();
                case AddictionType.Caffeine:
                    return new CaffeineAddiction();
                default:
                    throw new NotSupportedException($"AddictionType \"{addictionType}\" is not supported.");
            }
        }

        public static (AddictionType addictionType, float? potency)? GetAddictionTypeAndPotency(Interactable interactable)
        {
            var ri = interactable.preset.retailItem;

            if (ri.drunk > 0)
            {
                return (AddictionType.Alcohol, ri.drunk);
            }
            else if (ri.numb > 0 || ri.desireCategory == CompanyPreset.CompanyCategory.medical)
            {
                if (!OpioidAddiction.ExcludedItems.Contains(interactable.preset.name))
                    return (AddictionType.Opioid, ri.numb > 0f ? (1.0f + ri.numb) : null);
            }
            else if (SugarAddiction.Sugars.TryGetValue(interactable.preset.name, out var potency))
            {
                return (AddictionType.Sugar, potency);
            }
            else if (ri.desireCategory == CompanyPreset.CompanyCategory.caffeine)
            {
                if (interactable.preset.name == "MugCoffee" || interactable.preset.name == "TakeawayCoffee")
                    return (AddictionType.Caffeine, null);
            }

            if (Plugin.Instance.Config.DebugMode)
                Plugin.Log.LogInfo($"Not supported interactable \"{interactable.preset.name}\" consumed, skipped.");

            return null;
        }
    }
}
