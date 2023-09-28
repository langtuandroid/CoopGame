using System.Collections.Generic;
using Main.Scripts.Skills.Common.Component.Config.Follow;
using UnityEngine;

namespace Main.Scripts.Skills.Common.Component.Config
{
    [CreateAssetMenu(fileName = "SkillConfig", menuName = "Skill/SkillConfig")]
    public class SkillConfig : ScriptableObject
    {
        [SerializeField]
        private SkillSpawnPointType spawnPointType;
        [SerializeField]
        private SkillSpawnDirectionType spawnDirectionType;
        [SerializeField]
        private SkillDirectionType followDirectionType;
        [SerializeField]
        private SkillFollowStrategyBase followStrategy = default!;
        [SerializeField]
        private List<SkillTriggerPack> triggerPacks = new();
        [SerializeField]
        [Min(0f)]
        private float durationSec;
        [SerializeField]
        private SkillInterruptStrategy interruptStrategy;

        public SkillSpawnPointType SpawnPointType => spawnPointType;
        public SkillSpawnDirectionType SpawnDirectionType => spawnDirectionType;
        public SkillDirectionType FollowDirectionType => followDirectionType;
        public SkillFollowStrategyBase FollowStrategy => followStrategy;
        public List<SkillTriggerPack> TriggerPacks => triggerPacks;
        public float DurationSec => durationSec;
        public SkillInterruptStrategy InterruptStrategy => interruptStrategy;
    }
}