using UnityEngine;

namespace Main.Scripts.Skills.Common.Component.Config.Action
{
    [CreateAssetMenu(fileName = "SpawnAction", menuName = "Skill/Action/SpawnConfig")]
    public class SpawnConfigSkillAction : SpawnSkillActionBase
    {
        [SerializeField]
        private SkillConfig skillConfig;

        public SkillConfig SkillConfig => skillConfig;
    }
}