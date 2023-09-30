using System.Collections.Generic;
using UnityEngine;

namespace Main.Scripts.Skills.Common.Component.Config.Trigger
{
    [CreateAssetMenu(fileName = "ModifiableTriggers", menuName = "Skill/Trigger/ModifiableTriggers")]
    public class ModifiableSkillActionTriggers : SkillActionTriggerBase
    {
        [SerializeField]
        private List<ModifiableItem<SkillActionTriggerBase>> modifiableTriggers = new();

        public List<ModifiableItem<SkillActionTriggerBase>> ModifiableTriggers => modifiableTriggers;
    }
}