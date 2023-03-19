using Main.Scripts.Actions;
using UnityEngine;

namespace Main.Scripts.Skills.PassiveSkills.Modifiers
{
    [CreateAssetMenu(fileName = "HealConstantModifier", menuName = "Scriptable/HealConstantModifier")]
    public class HealConstantModifier : BaseModifier, HealModifier
    {
        [SerializeField]
        private uint healValue;

        public void ApplyHeal(Healable healableObject)
        {
            healableObject.ApplyHeal(healValue);
        }
    }
}