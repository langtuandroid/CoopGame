using Main.Scripts.Skills.Common.Controller;
using Main.Scripts.Skills.PassiveSkills.Triggers;
using UnityEngine;

namespace Main.Scripts.Skills.PassiveSkills
{
    public class PassiveSkillController : SkillController
    {
        [SerializeField]
        private PassiveSkillTriggerBase passiveSkillTrigger = default!;

        public PassiveSkillTriggerBase PassiveSkillTrigger => passiveSkillTrigger;
    }
}