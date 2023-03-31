using System;
using Main.Scripts.Actions.Health;
using UnityEngine;

namespace Main.Scripts.Effects.PeriodicEffects.Handlers.Damage
{
    public class DamagePeriodicEffectsHandler : PeriodicEffectsHandler
    {
        private Damageable? damageableTarget;

        public void TrySetTarget(GameObject targetObject)
        {
            damageableTarget = targetObject.GetComponent<Damageable>();
        }

        public void HandleEffect(PeriodicEffectBase periodicEffect, int stackCount)
        {
            if (damageableTarget == null) return;

            if (periodicEffect is not DamagePeriodicEffect damageEffect)
            {
                throw new ArgumentException(
                    $"Periodic effect \"{periodicEffect.NameId}\" with type {periodicEffect.PeriodicEffectType} is not DamagePeriodicEffect");
            }

            var damage = damageEffect.ConstantDamage * stackCount +
                         damageableTarget.GetMaxHealth() * damageEffect.PercentMaxHealthDamage * stackCount * 0.01f;
            damageableTarget.ApplyDamage(damage);
        }
    }
}