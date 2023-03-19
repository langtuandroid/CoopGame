using Main.Scripts.Actions;
using UnityEngine;

namespace Main.Scripts.Skills.PassiveSkills.Effects.Damage
{
    [CreateAssetMenu(fileName = "DamagePercentEffect", menuName = "Scriptable/DamagePercentEffect")]
    public class DamagePercentEffect : BaseEffect, DamageEffect
    {
        [SerializeField]
        [Min(0f)]
        private float maxHealthMultiplierDamage;

        public void ApplyDamage(Damageable damageableObject)
        {
            damageableObject.ApplyDamage((uint)(damageableObject.GetMaxHealth() * maxHealthMultiplierDamage));
        }
    }
}