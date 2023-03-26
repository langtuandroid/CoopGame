using System;
using Main.Scripts.Actions.Health;
using UnityEngine;

namespace Main.Scripts.Effects.PeriodicEffects.Handlers.Heal
{
    public class HealPeriodicEffectsHandler : PeriodicEffectsHandler
    {
        private Healable? healableTarget;

        public void TrySetTarget(GameObject targetObject)
        {
            healableTarget = targetObject.GetComponent<Healable>();
        }

        public void HandleEffect(PeriodicEffectBase periodicEffect)
        {
            if (healableTarget == null) return;

            if (periodicEffect is not HealPeriodicEffect healEffect)
            {
                throw new ArgumentException($"Periodic effect {periodicEffect.Id} with type {periodicEffect.PeriodicEffectType} is not HealPeriodicEffect");
            }

            var heal = healEffect.ConstantHeal +
                       healableTarget.GetMaxHealth() * healEffect.PercentMaxHealthHeal * 0.01f;
            healableTarget.ApplyHeal(heal);
        }
    }
}