using Main.Scripts.Actions;
using UnityEngine;

namespace Main.Scripts.Skills.PassiveSkills.Effects.Damage
{
    [CreateAssetMenu(fileName = "DamageConstantEffect", menuName = "Scriptable/DamageConstantEffect")]
    public class DamageConstantEffect : BaseEffect, DamageEffect
    {
        [SerializeField]
        private uint damage;

        public void ApplyDamage(Damageable damageableObject)
        {
            damageableObject.ApplyDamage(damage);
        }
    }
}