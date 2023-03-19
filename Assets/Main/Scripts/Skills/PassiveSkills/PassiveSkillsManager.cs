using System.Collections.Generic;
using Main.Scripts.Actions;
using Main.Scripts.Skills.PassiveSkills.Modifiers;
using UnityEngine;

namespace Main.Scripts.Skills.PassiveSkills
{
    public class PassiveSkillsManager : MonoBehaviour
    {
        [SerializeField]
        private List<PassiveSkill> passiveSkills = default!;

        private Healable healableObject = default!;
        private Damageable damageableObject = default!;

        private void Awake()
        {
            healableObject = GetComponent<Healable>();
            damageableObject = GetComponent<Damageable>();
        }

        public void ApplyModifiers(int tick, int tickRate)
        {
            foreach (var passiveSkill in passiveSkills)
            {
                foreach (var passiveSkillModifier in passiveSkill.passiveSkillModifiers)
                {
                    if (tick % (int)(tickRate / passiveSkillModifier.Frequency) != 0) continue;

                    TryModifyHealable(passiveSkillModifier);
                    TryModifyDamageable(passiveSkillModifier);
                }
            }
        }

        private void TryModifyHealable(BaseModifier baseModifier)
        {
            if (baseModifier is not HealModifier healModifier) return;

            healModifier.ApplyHeal(healableObject);
        }

        private void TryModifyDamageable(BaseModifier baseModifier)
        {
            if (baseModifier is not DamageableModifier healModifier) return;

            healModifier.ApplyDamage(damageableObject);
        }
    }
}