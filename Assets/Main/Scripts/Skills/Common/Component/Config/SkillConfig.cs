using System.Collections.Generic;
using Main.Scripts.Skills.Common.Component.Config.Follow;
using Main.Scripts.Skills.Common.Component.Config.Value;
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
        private bool startNewExecutionCharging;
        [SerializeField]
        private int[] executionChargeStepValues = null!;
        [SerializeField]
        private bool resetClicksCount;
        [SerializeField]
        private SkillValue durationTicks = null!;
        [SerializeField]
        private bool disableMoveWhileRunning;
        [SerializeField]
        private bool continueRunningWhileHolding;
        [SerializeField]
        private bool interruptWithSkillController;


        public SkillSpawnPointType SpawnPointType => spawnPointType;
        public SkillSpawnDirectionType SpawnDirectionType => spawnDirectionType;
        public SkillDirectionType FollowDirectionType => followDirectionType;
        public SkillFollowStrategyBase FollowStrategy => followStrategy;
        public List<SkillTriggerPack> TriggerPacks => triggerPacks;
        public bool StartNewExecutionCharging => startNewExecutionCharging;
        public int[] ExecutionChargeStepValues => executionChargeStepValues;
        public bool ResetClicksCount => resetClicksCount;
        public SkillValue DurationTicks => durationTicks;
        public bool DisableMoveWhileRunning => disableMoveWhileRunning;
        public bool ContinueRunningWhileHolding => continueRunningWhileHolding;
        public bool InterruptWithSkillController => interruptWithSkillController;
    }
}