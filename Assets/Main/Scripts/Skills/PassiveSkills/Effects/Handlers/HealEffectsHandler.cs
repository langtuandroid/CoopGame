using System.Collections.Generic;
using Main.Scripts.Actions;
using Main.Scripts.Skills.PassiveSkills.Effects.Heal;
using UnityEngine;

namespace Main.Scripts.Skills.PassiveSkills.Effects.Handlers
{
    public class HealEffectsHandler : EffectsHandler
    {
        private Healable? healableTarget;
        private List<HealEffect> healEffects = new();
        
        public bool TrySetTarget(GameObject targetObject)
        {
            return targetObject.TryGetComponent<Healable>(out healableTarget);
        }

        public bool TryRegisterEffect(BaseEffect effect)
        {
            if (effect is HealEffect healEffect)
            {
                healEffects.Add(healEffect);
                return true;
            }

            return false;
        }

        public void HandleEffects(int tick, int tickRate)
        {
            if (healableTarget == null) return;
            
            foreach (var healEffect in healEffects)
            {
                if (tick % (int)(tickRate / healEffect.GetFrequency()) != 0) continue;
                
                healEffect.ApplyHeal(healableTarget);
            }
        }
    }
}