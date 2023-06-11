using System.Collections.Generic;
using UnityEngine;

namespace Main.Scripts.Skills.Common.Component.Config.Follow
{
    [CreateAssetMenu(fileName = "ModifiableFollowStrategiesPack", menuName = "Skill/Follow/ModifiablePack")]
    public class ModifiableSkillFollowStrategyPack : SkillFollowStrategyBase
    {
        [SerializeField]
        private List<ModifiableItem<SkillFollowStrategyBase>> modifiablePacks = new();

        public List<ModifiableItem<SkillFollowStrategyBase>> ModifiablePacks => modifiablePacks;
    }
}