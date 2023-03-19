using System.Collections.Generic;
using Main.Scripts.Skills.PassiveSkills.Effects.Handlers;
using UnityEngine;

namespace Main.Scripts.Skills.PassiveSkills
{
    public class PassiveSkillsManager : MonoBehaviour
    {
        [SerializeField]
        private List<PassiveSkill> passiveSkills = default!;
        [SerializeField]
        private GameObject targetObject = default!;

        private List<EffectsHandler> effectsHandlers = new();

        private void Awake()
        {
            effectsHandlers.Add(new HealEffectsHandler());
            effectsHandlers.Add(new DamageEffectsHandler());

            foreach (var effectsHandler in effectsHandlers)
            {
                effectsHandler.TrySetTarget(targetObject);
                foreach (var passiveSkill in passiveSkills)
                {
                    foreach (var effect in passiveSkill.passiveSkillEffects)
                    {
                        effectsHandler.TryRegisterEffect(effect);
                    }
                }
            }
        }

        public void HandleEffects(int tick, int tickRate)
        {
            foreach (var effectsHandler in effectsHandlers)
            {
                effectsHandler.HandleEffects(tick, tickRate);
            }
        }
    }
}