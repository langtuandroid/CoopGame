using UnityEngine;

namespace Main.Scripts.Effects.PeriodicEffects
{
    public abstract class PeriodicEffectBase : EffectBase
    {
        [SerializeField]
        private PeriodicEffectType periodicEffectType;
        [SerializeField]
        private float frequency = 1f;

        public PeriodicEffectType PeriodicEffectType => periodicEffectType;
        public float Frequency => frequency;
    }
}