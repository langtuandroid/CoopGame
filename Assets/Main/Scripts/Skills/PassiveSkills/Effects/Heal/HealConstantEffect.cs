using Main.Scripts.Actions;
using UnityEngine;

namespace Main.Scripts.Skills.PassiveSkills.Effects.Heal
{
    [CreateAssetMenu(fileName = "HealConstantEffect", menuName = "Scriptable/HealConstantEffect")]
    public class HealConstantEffect : BaseEffect, HealEffect
    {
        [SerializeField]
        private uint healValue;

        public void ApplyHeal(Healable healableObject)
        {
            healableObject.ApplyHeal(healValue);
        }
    }
}