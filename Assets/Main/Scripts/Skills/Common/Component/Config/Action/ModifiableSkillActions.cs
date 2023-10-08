using Main.Scripts.Modifiers;
using UnityEngine;

namespace Main.Scripts.Skills.Common.Component.Config.Action
{
    [CreateAssetMenu(fileName = "ModifiableActions", menuName = "Skill/Action/ModifiableActions")]
    public class ModifiableSkillActions : SkillActionBase
    {
        [SerializeField]
        private ModifierId modifierId = null!;
        [SerializeField]
        private SerializableList<SkillActionBase>[] actionsByModifierLevel = null!;

        public ModifierId ModifierId => modifierId;
        public SerializableList<SkillActionBase>[] ActionsByModifierLevel => actionsByModifierLevel;

        private void OnValidate()
        {
            actionsByModifierLevel = ModifiableItemValidationHelper.GetLimitedArray(modifierId, actionsByModifierLevel);
        }
    }
}