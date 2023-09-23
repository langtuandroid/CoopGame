using System.Collections.Generic;
using Main.Scripts.Skills.Common.Component.Config.Action;
using Main.Scripts.Skills.Common.Component.Config.FindTargets;
using Main.Scripts.Skills.Common.Component.Config.Trigger;
using UnityEngine;

namespace Main.Scripts.Skills.Common.Component.Config.ActionsPack
{
    [CreateAssetMenu(fileName = "SkillActionsPack", menuName = "Skill/SkillActionsPack")]
    public class SkillActionsPack : ScriptableObject
    {
        [SerializeField]
        private SkillActionTriggerBase actionTrigger = default!;
        [SerializeField]
        private List<SkillFindTargetsStrategyBase> findTargetsStrategies = new();
        [SerializeField]
        private List<SkillActionBase> actions = default!;
        
        public SkillActionTriggerBase ActionTrigger => actionTrigger;
        public List<SkillFindTargetsStrategyBase> FindTargetsStrategies => findTargetsStrategies;
        public List<SkillActionBase> Actions => actions;
    }
}