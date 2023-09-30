using System.Collections.Generic;
using UnityEngine;

namespace Main.Scripts.Skills.Common.Component.Config.Follow
{
    [CreateAssetMenu(fileName = "ModifiableFollowStrategies", menuName = "Skill/Follow/ModifiableFollowStrategies")]
    public class ModifiableSkillFollowStrategies : SkillFollowStrategyBase
    {
        [SerializeField]
        private List<ModifiableItem<SkillFollowStrategyBase>> modifiableFollowStrategies = new();

        public List<ModifiableItem<SkillFollowStrategyBase>> ModifiableFollowStrategies => modifiableFollowStrategies;
    }
}