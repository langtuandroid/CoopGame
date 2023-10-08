using Main.Scripts.Modifiers;
using UnityEngine;

namespace Main.Scripts.Skills.Common.Component.Config.Trigger
{
    [CreateAssetMenu(fileName = "ModifiableTriggers", menuName = "Skill/Trigger/ModifiableTriggers")]
    public class ModifiableSkillActionTriggers : SkillActionTriggerBase
    {
        [SerializeField]
        private ModifierId modifierId = null!;
        [SerializeField]
        private SkillActionTriggerBase[] triggerByModifierLevel = null!;

        public ModifierId ModifierId => modifierId;
        public SkillActionTriggerBase[] TriggerByModifierLevel => triggerByModifierLevel;

        private void OnValidate()
        {
            triggerByModifierLevel =
                ModifiableItemValidationHelper.GetLimitedArray(modifierId, triggerByModifierLevel);
        }
    }
}