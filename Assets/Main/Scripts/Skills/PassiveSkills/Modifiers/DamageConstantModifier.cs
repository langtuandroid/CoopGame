using Main.Scripts.Actions;
using UnityEngine;

namespace Main.Scripts.Skills.PassiveSkills.Modifiers
{
    [CreateAssetMenu(fileName = "DamageConstantModifier", menuName = "Scriptable/DamageConstantModifier")]
    public class DamageConstantModifier : BaseModifier, DamageableModifier
    {
        [SerializeField]
        private uint damage;

        public void ApplyDamage(Damageable damageableObject)
        {
            damageableObject.ApplyDamage(damage);
        }
    }
}