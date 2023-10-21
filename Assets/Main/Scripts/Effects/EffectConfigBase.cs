using UnityEngine;

namespace Main.Scripts.Effects
{
    public abstract class EffectConfigBase : ScriptableObject
    {
        [SerializeField]
        [Min(1)]
        private int maxStackCount = 1;
        [SerializeField]
        [Min(0)]
        private int durationTicks;

        public string NameId => name;
        public int MaxStackCount => maxStackCount;
        public int DurationTicks => durationTicks;

        public bool IsUnlimited => DurationTicks == 0;
    }
}