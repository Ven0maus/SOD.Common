using SOD.Narcotics.AddictionCore.Addictions;
using System;

namespace SOD.Narcotics.AddictionCore
{
    public static class AddictionFactory
    {
        public static Addiction Get(int humanId, AddictionType addictionType)
        {
            // No fancy reflection here, doesn't work with il2cpp for some reason
            switch (addictionType)
            {
                case AddictionType.Alcohol:
                    return new AlcoholAddiction(humanId);
                case AddictionType.Nicotine:
                    return new NicotineAddiction(humanId);
                case AddictionType.Opioid:
                    return new OpioidAddiction(humanId);
                case AddictionType.Sugar:
                    return new SugarAddiction(humanId);
                case AddictionType.Caffeine:
                    return new CaffeineAddiction(humanId);
                default:
                    throw new NotSupportedException($"AddictionType \"{addictionType}\" is not supported.");
            }
        }
    }
}
