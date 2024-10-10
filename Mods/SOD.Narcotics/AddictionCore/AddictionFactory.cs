using SOD.Narcotics.AddictionCore.Addictions;
using System;
using System.Collections.Generic;

namespace SOD.Narcotics.AddictionCore
{
    public static class AddictionFactory
    {
        private static readonly Dictionary<AddictionType, Type> _addictionTypes = new()
        {
            { AddictionType.Alcohol, typeof(AlcoholAddiction) },
            { AddictionType.Nicotine, typeof(NicotineAddiction) },
            { AddictionType.Opioid, typeof(OpioidAddiction) }
        };

        public static Addiction Get(int humanId, AddictionType addictionType)
        {
            if (!_addictionTypes.TryGetValue(addictionType, out var type))
                throw new NotSupportedException($"AddictionType \"{addictionType}\" is not supported.");

            return (Addiction)Activator.CreateInstance(type, new[] {humanId});
        }
    }
}
