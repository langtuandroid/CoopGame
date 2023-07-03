using UnityEngine;

namespace Main.Scripts.Effects.PeriodicEffects.Handlers
{
    public interface PeriodicEffectsHandler
    {
        void TrySetTarget(object targetObject);
        void HandleEffect(PeriodicEffectBase periodicEffect, int stackCount);
    }
}