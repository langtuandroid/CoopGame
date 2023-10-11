using Main.Scripts.Modifiers;
using Main.Scripts.Utils;
using UnityEngine;

namespace Main.Scripts.Skills.Common.Component.Config.FindTargets
{
    [CreateAssetMenu(fileName = "ModifiableTargetsStrategies", menuName = "Skill/FindTargets/ModifiableTargetsStrategies")]
    public class ModifiableSkillFindTargetsStrategies : SkillFindTargetsStrategyBase
    {
        [SerializeField]
        private ModifierBase modifier = null!;
        [SerializeField]
        private SerializableList<SkillFindTargetsStrategyBase>[] findTargetsStrategiesByModifierLevel = null!;

        public ModifierBase Modifier => modifier;
        public SerializableList<SkillFindTargetsStrategyBase>[] FindTargetsStrategiesByModifierLevel => findTargetsStrategiesByModifierLevel;

        private void OnValidate()
        {
            findTargetsStrategiesByModifierLevel =
                ModifiableItemValidationHelper.GetLimitedArray(modifier, findTargetsStrategiesByModifierLevel);
        }
    }
}