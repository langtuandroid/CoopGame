using Main.Scripts.Actions;
using UnityEngine;

namespace Main.Scripts.Skills.PassiveSkills.Modifiers
{
    [CreateAssetMenu(fileName = "DamagePercentModifier", menuName = "Scriptable/DamagePercentModifier")]
    public class DamagePercentModifier : BaseModifier, DamageableModifier
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