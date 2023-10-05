using Main.Scripts.Skills.ActiveSkills;
using UnityEngine;

namespace Main.Scripts.Mobs.Config.Block.Action
{
    [CreateAssetMenu(fileName = "ActivateSkillActionBlock", menuName = "Mobs/LogicBlocks/Action/ActivateSkill")]
    public class ActivateSkillMobActionBlock : ActionMobBlockConfigBase
    {
        [SerializeField]
        private ActiveSkillType skillType;

        public ActiveSkillType SkillType => skillType;
    }
}