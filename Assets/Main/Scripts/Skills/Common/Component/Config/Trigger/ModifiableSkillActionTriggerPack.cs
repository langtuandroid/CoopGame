using System.Collections.Generic;
using UnityEngine;

namespace Main.Scripts.Skills.Common.Component.Config.Trigger
{
    [CreateAssetMenu(fileName = "ModifiableActionTriggersPack", menuName = "Skill/Trigger/ModifiablePack")]
    public class ModifiableSkillActionTriggerPack : SkillActionTriggerBase
    {
        [SerializeField]
        private List<ModifiableItem<SkillActionTriggerBase>> modifiablePacks = new();

        public List<ModifiableItem<SkillActionTriggerBase>> ModifiablePacks => modifiablePacks;
    }
}