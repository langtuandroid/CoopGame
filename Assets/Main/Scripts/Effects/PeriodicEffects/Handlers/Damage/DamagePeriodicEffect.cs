using UnityEngine;

namespace Main.Scripts.Effects.PeriodicEffects.Handlers.Damage
{
    [CreateAssetMenu(fileName = "DamagePeriodicEffect", menuName = "Scriptable/Effects/Periodic/DamagePeriodicEffect")]
    public class DamagePeriodicEffect : PeriodicEffectBase
    {
        [SerializeField]
        private float constantDamage;
        [SerializeField]
        private float percentMaxHealthDamage;

        public float ConstantDamage => constantDamage;
        public float PercentMaxHealthDamage => percentMaxHealthDamage;
    }
}