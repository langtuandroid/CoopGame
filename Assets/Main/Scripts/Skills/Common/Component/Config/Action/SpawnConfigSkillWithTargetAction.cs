using UnityEngine;

namespace Main.Scripts.Skills.Common.Component.Config.Action
{
    [CreateAssetMenu(fileName = "SpawnConfigWithTargetAction", menuName = "Skill/Action/SpawnConfigWithTarget")]
    public class SpawnConfigWithTargetSkillAction : SpawnSkillActionBase
    {
        [SerializeField]
        private SkillConfig skillConfig = default!;

        public SkillConfig SkillConfig => skillConfig;
    }
}