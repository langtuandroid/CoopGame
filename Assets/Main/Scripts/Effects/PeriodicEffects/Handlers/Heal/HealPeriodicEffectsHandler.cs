using System;
using Main.Scripts.Actions.Data;
using Main.Scripts.Actions.Health;

namespace Main.Scripts.Effects.PeriodicEffects.Handlers.Heal
{
    public class HealPeriodicEffectsHandler : PeriodicEffectsHandler
    {
        private Healable? healableTarget;

        public void TrySetTarget(object targetObject)
        {
            healableTarget = targetObject as Healable;
        }

        public void HandleEffect(PeriodicEffectBase periodicEffect, int stackCount)
        {
            if (healableTarget == null) return;

            if (periodicEffect is not HealPeriodicEffect healEffect)
            {
                throw new ArgumentException($"Periodic effect {periodicEffect.NameId} with type {periodicEffect.PeriodicEffectType} is not HealPeriodicEffect");
            }

            var heal = healEffect.ConstantHeal * stackCount +
                       healableTarget.GetMaxHealth() * healEffect.PercentMaxHealthHeal * stackCount * 0.01f;
            var healActionData = new HealActionData
            {
                healOwner = null,
                healValue = heal
            };
            healableTarget.AddHeal(ref healActionData);
        }
    }
}