using UnityEngine;

namespace Main.Scripts.Skills.Common.Component.Config.Action
{
    [CreateAssetMenu(fileName = "SpawnConfigAction", menuName = "Skill/Action/SpawnConfig")]
    public class SpawnConfigSkillAction : SpawnSkillActionBase
    {
        [SerializeField]
        private SkillConfig skillConfig = default!;

        public SkillConfig SkillConfig => skillConfig;
    }
}