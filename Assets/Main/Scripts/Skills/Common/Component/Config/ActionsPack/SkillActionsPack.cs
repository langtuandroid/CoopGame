using System.Collections.Generic;
using Main.Scripts.Skills.Common.Component.Config.Action;
using Main.Scripts.Skills.Common.Component.Config.FindTargets;
using UnityEngine;

namespace Main.Scripts.Skills.Common.Component.Config.ActionsPack
{
    [CreateAssetMenu(fileName = "ActionsPack", menuName = "Skill/ActionsPack")]
    public class SkillActionsPack : ScriptableObject
    {
        [SerializeField]
        private List<SkillFindTargetsStrategyBase> findTargetsStrategies = new();
        [SerializeField]
        private List<SkillActionBase> actions = default!;

        public List<SkillFindTargetsStrategyBase> FindTargetsStrategies => findTargetsStrategies;
        public List<SkillActionBase> Actions => actions;
    }
}