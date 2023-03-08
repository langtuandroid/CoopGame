using System;
using Fusion;
using UnityEngine;

namespace Main.Scripts.Weapon
{
    public class ActiveSkillManager : NetworkBehaviour
    {
        [SerializeField]
        private ActiveSkillBase primarySkill = default!;

        public bool ActivateSkill(ActiveSkillType skillType, PlayerRef owner)
        {
            if (!IsSkillActivatingAllowed(skillType))
                return false;

            var skill = getSkillByType(skillType);

            return skill.Activate(owner);
        }

        public bool IsSkillRunning(ActiveSkillType skillType)
        {
            return getSkillByType(skillType).IsRunning();
        }

        private bool IsSkillActivatingAllowed(ActiveSkillType skillType)
        {
            return !primarySkill.IsRunning();
        }

        private ActiveSkillBase getSkillByType(ActiveSkillType skillType)
        {
            switch (skillType)
            {
                case ActiveSkillType.PRIMARY:
                    return primarySkill;
                default:
                    throw new ArgumentOutOfRangeException(nameof(skillType), skillType, null);
            }
        }
    }
}