using Main.Scripts.Modifiers;
using Main.Scripts.Utils;
using UnityEngine;

namespace Main.Scripts.Skills.Common.Component.Config.FindTargets
{
    [CreateAssetMenu(fileName = "ModifiableTargetsStrategies", menuName = "Skill/FindTargets/ModifiableTargetsStrategies")]
    public class ModifiableSkillFindTargetsStrategies : SkillFindTargetsStrategyBase
    {
        [SerializeField]
        private ModifierId modifierId = null!;
        [SerializeField]
        private SerializableList<SkillFindTargetsStrategyBase>[] findTargetsStrategiesByModifierLevel = null!;

        public ModifierId ModifierId => modifierId;
        public SerializableList<SkillFindTargetsStrategyBase>[] FindTargetsStrategiesByModifierLevel => findTargetsStrategiesByModifierLevel;

        private void OnValidate()
        {
            findTargetsStrategiesByModifierLevel =
                ModifiableItemValidationHelper.GetLimitedArray(modifierId, findTargetsStrategiesByModifierLevel);
        }
    }
}