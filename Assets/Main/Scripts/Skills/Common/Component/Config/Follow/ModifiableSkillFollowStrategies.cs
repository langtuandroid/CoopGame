using Main.Scripts.Modifiers;
using UnityEngine;

namespace Main.Scripts.Skills.Common.Component.Config.Follow
{
    [CreateAssetMenu(fileName = "ModifiableFollowStrategies", menuName = "Skill/Follow/ModifiableFollowStrategies")]
    public class ModifiableSkillFollowStrategies : SkillFollowStrategyBase
    {
        [SerializeField]
        private ModifierId modifierId = null!;
        [SerializeField]
        private SkillFollowStrategyBase[] followStrategyByModifierLevel = null!;

        public ModifierId ModifierId => modifierId;
        public SkillFollowStrategyBase[] FollowStrategyByModifierLevel => followStrategyByModifierLevel;

        private void OnValidate()
        {
            followStrategyByModifierLevel =
                ModifiableItemValidationHelper.GetLimitedArray(modifierId, followStrategyByModifierLevel);
        }
    }
}