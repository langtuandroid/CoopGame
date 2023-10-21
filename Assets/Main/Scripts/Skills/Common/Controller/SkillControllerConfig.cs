using System.Collections.Generic;
using Main.Scripts.Player.InputSystem.Target;
using Main.Scripts.Skills.Common.Component.Config;
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
        [Min(0)]
        private int castDurationTicks;
        [SerializeField]
        private bool disableMoveOnCast;
        [SerializeField]
        [Min(0)]
        private int executionDurationTicks;
        [SerializeField]
        private bool disableMoveOnExecution;
        [SerializeField]
        [Min(0)]
        private int cooldownTicks;

        public List<SkillConfig> RunOnCastSkillConfigs => runOnCastSkillConfigs;
        public List<SkillConfig> RunOnExecutionSkillConfigs => runOnExecutionSkillConfigs;
        public SkillActivationType ActivationType => activationType;
        public GameObject? AreaMarker => areaMarker;
        public UnitTargetType SelectionTargetType => selectionTargetType;
        public int TicksToFullPowerCharge => ticksForFullPowerCharge;
        public int[] PowerChargeStepValues => powerChargeStepValues;
        public int CastDurationTicks => castDurationTicks;
        public bool DisableMoveOnCast => disableMoveOnCast;
        public int ExecutionDurationTicks => executionDurationTicks;
        public bool DisableMoveOnExecution => disableMoveOnExecution;
        public int CooldownTicks => cooldownTicks;
    }
}