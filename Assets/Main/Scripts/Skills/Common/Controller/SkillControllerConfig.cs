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
        [Min(0f)]
        private float executionDurationSec;
        [SerializeField]
        private bool disableMoveOnExecution;
        [SerializeField]
        [Min(0f)]
        private float castDurationSec;
        [SerializeField]
        private bool disableMoveOnCast;
        [SerializeField]
        [Min(0f)]
        private float cooldownSec;

        public List<SkillConfig> RunOnCastSkillConfigs => runOnCastSkillConfigs;
        public List<SkillConfig> RunOnExecutionSkillConfigs => runOnExecutionSkillConfigs;
        public SkillActivationType ActivationType => activationType;
        public GameObject? AreaMarker => areaMarker;
        public UnitTargetType SelectionTargetType => selectionTargetType;
        public float ExecutionDurationSec => executionDurationSec;
        public bool DisableMoveOnExecution => disableMoveOnExecution;
        public float CastDurationSec => castDurationSec;
        public bool DisableMoveOnCast => disableMoveOnCast;
        public float CooldownSec => cooldownSec;
    }
}