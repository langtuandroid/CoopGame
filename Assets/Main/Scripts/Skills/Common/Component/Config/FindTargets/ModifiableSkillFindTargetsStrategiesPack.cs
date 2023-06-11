using System.Collections.Generic;
using UnityEngine;

namespace Main.Scripts.Skills.Common.Component.Config.FindTargets
{
    [CreateAssetMenu(fileName = "ModifiableFindTargetsStrategiesPack", menuName = "Skill/FindTargets/ModifiablePack")]
    public class ModifiableSkillFindTargetsStrategiesPack : SkillFindTargetsStrategyBase
    {
        [SerializeField]
        private List<ModifiableList<SkillFindTargetsStrategyBase>> modifiablePacks = new();

        public List<ModifiableList<SkillFindTargetsStrategyBase>> ModifiablePacks => modifiablePacks;
    }
}