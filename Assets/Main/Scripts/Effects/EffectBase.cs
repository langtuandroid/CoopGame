using UnityEngine;

namespace Main.Scripts.Effects
{
    public abstract class EffectBase : ScriptableObject
    {
        [SerializeField]
        [Min(1)]
        private int maxStackCount = 1;
        [SerializeField]
        private float durationSec;

        public string NameId => name;
        public int MaxStackCount => maxStackCount;
        public float DurationSec => durationSec;
    }
}