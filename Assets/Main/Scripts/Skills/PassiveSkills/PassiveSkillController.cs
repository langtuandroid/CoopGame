using Main.Scripts.Skills.Common.Controller;
using Main.Scripts.Skills.PassiveSkills.Triggers;
using UnityEngine;

namespace Main.Scripts.Skills.PassiveSkills
{
    public class PassiveSkillController : SkillController
    {
        public PassiveSkillTriggerBase PassiveSkillTrigger { get; }

        public PassiveSkillController(
            PassiveSkillTriggerBase passiveSkillTrigger,
            SkillControllerConfig skillControllerConfig,
            Transform selfUnitTransform,
            LayerMask alliesLayerMask,
            LayerMask opponentsLayerMask
        ) : base(
            skillControllerConfig, selfUnitTransform, alliesLayerMask, opponentsLayerMask)
        {
            PassiveSkillTrigger = passiveSkillTrigger;
        }
    }
}