using System;
using Main.Scripts.Skills.Common.Controller;
using Main.Scripts.Skills.PassiveSkills.Triggers;

namespace Main.Scripts.Skills.PassiveSkills
{
    [Serializable]
    public struct PassiveSkillControllerData
    {
        public SkillControllerConfig SkillControllerConfig;
        public PassiveSkillTriggerBase PassiveSkillTrigger;
    }
}