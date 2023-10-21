using Main.Scripts.Effects.TriggerEffects.Triggers;
using Main.Scripts.Skills.Common.Controller;
using UnityEngine;

namespace Main.Scripts.Effects.TriggerEffects
{
    public class EffectSkillController : SkillController
    {
        public EffectTriggerBase PassiveSkillTrigger { get; private set; } = null!;

        public void Init(
            EffectTriggerBase passiveSkillTrigger,
            SkillControllerConfig skillControllerConfig,
            Transform selfUnitTransform,
            LayerMask alliesLayerMask,
            LayerMask opponentsLayerMask
        )
        {
            Init(skillControllerConfig, selfUnitTransform, alliesLayerMask, opponentsLayerMask);
            PassiveSkillTrigger = passiveSkillTrigger;
        }
    }
}