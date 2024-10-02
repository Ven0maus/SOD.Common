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
                case AddictionType.Nicotine:
                    return new NicotineAddiction();
                case AddictionType.Opioid:
                    return new OpioidAddiction();
            }

            throw new NotSupportedException($"AddictionType \"{addictionType}\" is not supported.");
        }
    }
}
