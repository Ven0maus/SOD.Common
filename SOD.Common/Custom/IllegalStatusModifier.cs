using System;

namespace SOD.Common.Custom
{
    internal sealed class IllegalStatusModifier
    {
        public IllegalStatusModifier(string key, float time, bool useTime)
        {
            Key = key;
            TimeRemainingSec = time;
            UseTime = useTime;
        }

        public string Key { get; }
        public float TimeRemainingSec { get; set; }
        public bool UseTime { get; }
    }
}