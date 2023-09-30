using System.Collections.Generic;
using UnityEngine;

namespace Main.Scripts.Skills.Common.Component.Config.Action
{
    [CreateAssetMenu(fileName = "ModifiableActions", menuName = "Skill/Action/ModifiableActions")]
    public class ModifiableSkillActions : SkillActionBase
    {
        [SerializeField]
        private List<ModifiableList<SkillActionBase>> modifiableActions = new();

        public List<ModifiableList<SkillActionBase>> ModifiableActions => modifiableActions;
    }
}