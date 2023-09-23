using System.Collections.Generic;
using Main.Scripts.Skills.Common.Component.Config.Action;
using Main.Scripts.Skills.Common.Component.Config.ActionsPack;
using Main.Scripts.Skills.Common.Component.Config.FindTargets;
using Main.Scripts.Skills.Common.Component.Config.Follow;
using Main.Scripts.Skills.Common.Component.Config.Trigger;
using UnityEngine;
using UnityEngine.Serialization;

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
        [FormerlySerializedAs("skillActionsPacks")]
        [SerializeField]
        private List<SkillActionsPack> actionsPacks = new();
        [SerializeField, Tooltip("Enable 'Destroy when state authority leaves'")]
        private bool isAffectTargetsOnlyOneTime;
        [SerializeField]
        [Min(0f)]
        private float durationSec;
        [SerializeField]
        private SkillInterruptStrategy interruptStrategy;
        [SerializeField]
        [Min(0f)]
        private float dontDestroyAfterFinishDurationSec;
        [SerializeField]
        private bool dontDestroyAfterStopAction;
        [SerializeField]
        private SkillComponent prefab = default!;

        public SkillSpawnPointType SpawnPointType => spawnPointType;
        public SkillSpawnDirectionType SpawnDirectionType => spawnDirectionType;
        public SkillDirectionType FollowDirectionType => followDirectionType;
        public SkillFollowStrategyBase FollowStrategy => followStrategy;
        public List<SkillActionsPack> ActionsPacks => actionsPacks;
        public bool IsAffectTargetsOnlyOneTime => isAffectTargetsOnlyOneTime;
        public float DurationSec => durationSec;
        public SkillInterruptStrategy InterruptStrategy => interruptStrategy;
        public float DontDestroyAfterFinishDurationSec => dontDestroyAfterFinishDurationSec;
        public bool DontDestroyAfterStopAction => dontDestroyAfterStopAction;
        public SkillComponent Prefab => prefab;
    }
}