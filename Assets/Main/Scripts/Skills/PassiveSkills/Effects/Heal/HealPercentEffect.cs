using Main.Scripts.Actions;
using UnityEngine;

namespace Main.Scripts.Skills.PassiveSkills.Effects.Heal
{
    [CreateAssetMenu(fileName = "HealPercentEffect", menuName = "Scriptable/HealPercentEffect")]
    public class HealPercentEffect : BaseEffect, HealEffect
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