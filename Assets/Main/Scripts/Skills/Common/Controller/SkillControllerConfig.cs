using System.Collections.Generic;
using Main.Scripts.Player.InputSystem.Target;
using Main.Scripts.Skills.Common.Component.Config;
using Main.Scripts.Skills.Common.Controller.Interruption;
using UnityEngine;

namespace Main.Scripts.Skills.Common.Controller
{
    [CreateAssetMenu(fileName = "SkillControllerConfig", menuName = "Skill/SkillControllerConfig")]
    public class SkillControllerConfig : ScriptableObject
    {
        [SerializeField]
        private List<SkillConfig> runOnCastSkillConfigs = new();
        [SerializeField]
        private List<SkillConfig> runOnExecutionSkillConfigs = new();
        [SerializeField]
        private SkillActivationType activationType;
        [SerializeField]
        private GameObject? areaMarker;
        [SerializeField]
        private UnitTargetType selectionTargetType;
        [SerializeField]
        [Min(0)]
        private int ticksForFullPowerCharge;
        [SerializeField]
        private int[] powerChargeStepValues = null!;
        [SerializeField]
        private SkillHoldingType holdingType;
        [SerializeField]
        [Min(0)]
        private int castDurationTicks;
        [SerializeField]
        private bool disableMoveOnCast;
        [SerializeField]
        private SkillInterruptionData castInterruptionData = SkillInterruptionData.DEFAULT;
        [SerializeField]
        [Min(1)]
        private int executionDurationTicks = 1;
        [SerializeField]
        private bool disableMoveOnExecution;
        [SerializeField]
        private SkillInterruptionData executionInterruptionData = SkillInterruptionData.DEFAULT;
        [SerializeField]
        [Min(0)]
        private int cooldownTicks;
        [SerializeField]
        private SkillCooldownStartType cooldownStartType = SkillCooldownStartType.OnExecute;

        public List<SkillConfig> RunOnCastSkillConfigs => runOnCastSkillConfigs;
        public List<SkillConfig> RunOnExecutionSkillConfigs => runOnExecutionSkillConfigs;
        public SkillActivationType ActivationType => activationType;
        public GameObject? AreaMarker => areaMarker;
        public UnitTargetType SelectionTargetType => selectionTargetType;
        public int TicksToFullPowerCharge => ticksForFullPowerCharge;
        public int[] PowerChargeStepValues => powerChargeStepValues;
        public SkillHoldingType HoldingType => holdingType;
        public int CastDurationTicks => castDurationTicks;
        public bool DisableMoveOnCast => disableMoveOnCast;
        public ref SkillInterruptionData CastInterruptionData => ref castInterruptionData;
        public int ExecutionDurationTicks => executionDurationTicks;
        public bool DisableMoveOnExecution => disableMoveOnExecution;
        public ref SkillInterruptionData ExecutionInterruptionData => ref executionInterruptionData;
        public int CooldownTicks => cooldownTicks;
        public SkillCooldownStartType CooldownStartType => cooldownStartType;
    }
}