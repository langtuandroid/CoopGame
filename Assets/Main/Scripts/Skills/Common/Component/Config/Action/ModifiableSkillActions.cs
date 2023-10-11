using Main.Scripts.Modifiers;
using Main.Scripts.Utils;
using UnityEngine;

namespace Main.Scripts.Skills.Common.Component.Config.Action
{
    [CreateAssetMenu(fileName = "ModifiableActions", menuName = "Skill/Action/ModifiableActions")]
    public class ModifiableSkillActions : SkillActionBase
    {
        [SerializeField]
        private ModifierBase modifier = null!;
        [SerializeField]
        private SerializableList<SkillActionBase>[] actionsByModifierLevel = null!;

        public ModifierBase Modifier => modifier;
        public SerializableList<SkillActionBase>[] ActionsByModifierLevel => actionsByModifierLevel;

        private void OnValidate()
        {
            actionsByModifierLevel = ModifiableItemValidationHelper.GetLimitedArray(modifier, actionsByModifierLevel);
        }
    }
}