using System.Collections.Generic;
using UnityEngine;

namespace Main.Scripts.Skills.Common.Component.Config.FindTargets
{
    [CreateAssetMenu(fileName = "ModifiableTargetsStrategies", menuName = "Skill/FindTargets/ModifiableTargetsStrategies")]
    public class ModifiableSkillFindTargetsStrategies : SkillFindTargetsStrategyBase
    {
        [SerializeField]
        private List<ModifiableList<SkillFindTargetsStrategyBase>> modifiableTargetsStrategies = new();

        public List<ModifiableList<SkillFindTargetsStrategyBase>> ModifiableTargetsStrategies => modifiableTargetsStrategies;
    }
}