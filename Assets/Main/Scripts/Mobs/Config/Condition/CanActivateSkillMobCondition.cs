using Main.Scripts.Skills.ActiveSkills;
using UnityEngine;

namespace Main.Scripts.Mobs.Config.Condition
{
    [CreateAssetMenu(fileName = "CanActivateSkillCondition", menuName = "Mobs/LogicBlocks/Condition/CanActivateSkill")]
    public class CanActivateSkillMobCondition : MobConditionConfigBase
    {
        [SerializeField]
        private ActiveSkillType skillType;

        public ActiveSkillType SkillType => skillType;
    }
}