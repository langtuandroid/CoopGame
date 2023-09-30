using System;
using Main.Scripts.Skills.Common.Controller;
using Main.Scripts.Skills.PassiveSkills.Triggers;

namespace Main.Scripts.Skills.PassiveSkills
{
    [Serializable]
    public struct PassiveSkillData
    {
        public PassiveSkillTriggerBase PassiveSkillTrigger;
        public SkillControllerConfig SkillControllerConfig;
    }
}