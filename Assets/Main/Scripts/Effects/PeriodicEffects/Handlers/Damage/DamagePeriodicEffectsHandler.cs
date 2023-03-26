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

        public void HandleEffect(PeriodicEffectBase periodicEffect)
        {
            if (damageableTarget == null) return;

            if (periodicEffect is not DamagePeriodicEffect damageEffect)
            {
                throw new ArgumentException(
                    $"Periodic effect \"{periodicEffect.Id}\" with type {periodicEffect.PeriodicEffectType} is not DamagePeriodicEffect");
            }

            var damage = damageEffect.ConstantDamage +
                         damageableTarget.GetMaxHealth() * damageEffect.PercentMaxHealthDamage * 0.01f;
            damageableTarget.ApplyDamage(damage);
        }
    }
}