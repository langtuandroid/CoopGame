using UnityEngine;

namespace Main.Scripts.Effects.PeriodicEffects.Handlers
{
    public interface PeriodicEffectsHandler
    {
        void TrySetTarget(GameObject targetObject);
        void HandleEffect(PeriodicEffectBase periodicEffect, int stackCount);
    }
}