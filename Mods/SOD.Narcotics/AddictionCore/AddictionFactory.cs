using SOD.Narcotics.AddictionCore.Addictions;
using System;

namespace SOD.Narcotics.AddictionCore
{
    public static class AddictionFactory
    {
        public static Addiction Get(AddictionType addictionType)
        {
            // No fancy reflection here, doesn't work with il2cpp for some reason
            switch (addictionType)
            {
                case AddictionType.Alcohol:
                    return new AlcoholAddiction();
                case AddictionType.Nicotine:
                    return new NicotineAddiction();
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
    }
}
