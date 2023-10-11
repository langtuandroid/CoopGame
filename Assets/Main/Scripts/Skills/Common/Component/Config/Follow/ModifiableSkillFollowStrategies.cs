using Main.Scripts.Modifiers;
using UnityEngine;

namespace Main.Scripts.Skills.Common.Component.Config.Follow
{
    [CreateAssetMenu(fileName = "ModifiableFollowStrategies", menuName = "Skill/Follow/ModifiableFollowStrategies")]
    public class ModifiableSkillFollowStrategies : SkillFollowStrategyBase
    {
        [SerializeField]
        private ModifierBase modifier = null!;
        [SerializeField]
        private SkillFollowStrategyBase[] followStrategyByModifierLevel = null!;

        public ModifierBase Modifier => modifier;
        public SkillFollowStrategyBase[] FollowStrategyByModifierLevel => followStrategyByModifierLevel;

        private void OnValidate()
        {
            followStrategyByModifierLevel =
                ModifiableItemValidationHelper.GetLimitedArray(modifier, followStrategyByModifierLevel);
        }
    }
}