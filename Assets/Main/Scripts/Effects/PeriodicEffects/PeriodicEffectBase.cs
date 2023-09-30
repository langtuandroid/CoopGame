using UnityEngine;

namespace Main.Scripts.Effects.PeriodicEffects
{
    public abstract class PeriodicEffectBase : EffectBase
    {
        [SerializeField]
        private PeriodicEffectType periodicEffectType;
        [SerializeField]
        [Min(1)]
        private int frequencyTicks = 24;

        public PeriodicEffectType PeriodicEffectType => periodicEffectType;
        public int FrequencyTicks => frequencyTicks;
    }
}