using System;
using Main.Scripts.Effects.TriggerEffects.Triggers;
using Main.Scripts.Skills.Common.Controller;
using UnityEngine;

namespace Main.Scripts.Effects.TriggerEffects
{
    [CreateAssetMenu(fileName = "TriggerEffectConfig", menuName = "Skill/Effects/TriggerEffect")]
    public class TriggerEffectConfig : EffectConfigBase
    {
        [SerializeField]
        private EffectTriggerBase trigger = null!;
        [SerializeField]
        private SkillControllerConfig skillControllerConfig = null!;

        public EffectTriggerBase Trigger => trigger;
        public SkillControllerConfig SkillControllerConfig => skillControllerConfig;

        private void OnValidate()
        {
            if (Trigger == null)
            {
                throw new Exception($"{name} TriggerEffectConfig trigger must be not null");
            }

            SkillConfigsValidationHelper.Validate(skillControllerConfig);
        }
    }
}