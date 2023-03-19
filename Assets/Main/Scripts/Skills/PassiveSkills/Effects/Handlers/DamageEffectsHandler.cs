using System.Collections.Generic;
using Main.Scripts.Actions;
using Main.Scripts.Skills.PassiveSkills.Effects.Damage;
using UnityEngine;

namespace Main.Scripts.Skills.PassiveSkills.Effects.Handlers
{
    public class DamageEffectsHandler : EffectsHandler
    {
        private Damageable? damageableTarget;
        private List<DamageEffect> damageEffects = new();

        public bool TrySetTarget(GameObject targetObject)
        {
            return targetObject.TryGetComponent<Damageable>(out damageableTarget);
        }

        public bool TryRegisterEffect(BaseEffect effect)
        {
            if (effect is DamageEffect damageEffect)
            {
                damageEffects.Add(damageEffect);
                return true;
            }

            return false;
        }

        public void HandleEffects(int tick, int tickRate)
        {
            if (damageableTarget == null) return;

            foreach (var damageEffect in damageEffects)
            {
                if (tick % (int)(tickRate / damageEffect.GetFrequency()) != 0) continue;

                damageEffect.ApplyDamage(damageableTarget);
            }
        }
    }
}