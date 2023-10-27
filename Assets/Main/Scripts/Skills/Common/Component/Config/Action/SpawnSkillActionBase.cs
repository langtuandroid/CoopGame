using UnityEngine;

namespace Main.Scripts.Skills.Common.Component.Config.Action
{
    public class SpawnSkillActionBase : SkillActionBase
    {
        [SerializeField]
        private SkillPointType spawnPointType;
        [SerializeField]
        private SkillDirectionType spawnDirectionType;

        public SkillPointType SpawnPointType => spawnPointType;
        public SkillDirectionType SpawnDirectionType => spawnDirectionType;
    }
}