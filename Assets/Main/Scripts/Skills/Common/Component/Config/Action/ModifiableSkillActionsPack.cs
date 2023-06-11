using System.Collections.Generic;
using UnityEngine;

namespace Main.Scripts.Skills.Common.Component.Config.Action
{
    [CreateAssetMenu(fileName = "ModifiableActionsPack", menuName = "Skill/Action/ModifiablePack")]
    public class ModifiableSkillActionsPack : SkillActionBase
    {
        [SerializeField]
        private List<ModifiableList<SkillActionBase>> modifiablePacks = new();

        public List<ModifiableList<SkillActionBase>> ModifiablePacks => modifiablePacks;
    }
}