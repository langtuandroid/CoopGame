using System.Collections.Generic;
using Main.Scripts.Skills.Common.Component.Config.Action;
using Main.Scripts.Skills.Common.Component.Config.FindTargets;
using Main.Scripts.Skills.Common.Component.Config.Follow;
using Main.Scripts.Skills.Common.Component.Config.Trigger;
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
        private SkillActionTriggerBase actionTrigger = default!;
        [SerializeField]
        private List<SkillFindTargetsStrategyBase> findTargetsStrategies = new();
        [SerializeField]
        private bool isAffectTargetsOnlyOneTime;
        [SerializeField]
        private List<SkillActionBase> actions = default!;
        [SerializeField]
        [Min(0f)]
        private float durationSec;
        [SerializeField]
        private SkillInterruptStrategy interruptStrategy;
        [SerializeField]
        private SkillComponent prefab = default!;

        public SkillSpawnPointType SpawnPointType => spawnPointType;
        public SkillSpawnDirectionType SpawnDirectionType => spawnDirectionType;
        public SkillDirectionType FollowDirectionType => followDirectionType;
        public SkillFollowStrategyBase FollowStrategy => followStrategy;
        public SkillActionTriggerBase ActionTrigger => actionTrigger;
        public List<SkillFindTargetsStrategyBase> FindTargetsStrategies => findTargetsStrategies;
        public bool IsAffectTargetsOnlyOneTime => isAffectTargetsOnlyOneTime;
        public List<SkillActionBase> Actions => actions;
        public float DurationSec => durationSec;
        public SkillInterruptStrategy InterruptStrategy => interruptStrategy;
        public SkillComponent Prefab => prefab;
    }
}