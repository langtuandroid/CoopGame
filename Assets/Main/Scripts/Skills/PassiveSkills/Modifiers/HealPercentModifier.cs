using Main.Scripts.Actions;
using UnityEngine;

namespace Main.Scripts.Skills.PassiveSkills.Modifiers
{
    [CreateAssetMenu(fileName = "HealPercentModifier", menuName = "Scriptable/HealPercentModifier")]
    public class HealPercentModifier : BaseModifier, HealModifier
    {
        [SerializeField]
        [Min(0f)]
        private float maxHealthMultiplierHeal;

        public void ApplyHeal(Healable healableObject)
        {
            healableObject.ApplyHeal((uint)(healableObject.GetMaxHealth() * maxHealthMultiplierHeal));
        }
    }
}