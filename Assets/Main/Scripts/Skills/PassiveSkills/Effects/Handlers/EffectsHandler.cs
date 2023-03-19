using UnityEngine;

namespace Main.Scripts.Skills.PassiveSkills.Effects.Handlers
{
    public interface EffectsHandler
    {
        bool TrySetTarget(GameObject targetObject);
        bool TryRegisterEffect(BaseEffect effect);
        void HandleEffects(int tick, int tickRate);
    }
}