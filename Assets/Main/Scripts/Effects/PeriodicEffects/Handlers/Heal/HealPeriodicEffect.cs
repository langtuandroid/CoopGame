using UnityEngine;

namespace Main.Scripts.Effects.PeriodicEffects.Handlers.Heal
{
    [CreateAssetMenu(fileName = "HealPeriodicEffect", menuName = "Scriptable/Effects/Periodic/HealPeriodicEffect")]
    public class HealPeriodicEffect : PeriodicEffectBase
    {
        [SerializeField]
        private float constantHeal;
        [SerializeField]
        private float percentMaxHealthHeal;

        public float ConstantHeal => constantHeal;
        public float PercentMaxHealthHeal => percentMaxHealthHeal;
    }
}