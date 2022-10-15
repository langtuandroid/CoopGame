using System;
using Fusion;
using UnityEngine;

namespace Main.Scripts.Weapon
{
    public class SkillManager : NetworkBehaviour
    {
        [SerializeField]
        private SkillBase primarySkill;

        public bool ActivateSkill(SkillType skillType, PlayerRef owner)
        {
            if (!IsSkillActivatingAllowed(skillType))
                return false;

            var skill = getSkillByType(skillType);

            return skill.Activate(owner);
        }

        public bool IsSkillRunning(SkillType skillType)
        {
            return getSkillByType(skillType).IsRunning();
        }

        private bool IsSkillActivatingAllowed(SkillType skillType)
        {
            return !primarySkill.IsRunning();
        }

        private SkillBase getSkillByType(SkillType skillType)
        {
            switch (skillType)
            {
                case SkillType.PRIMARY:
                    return primarySkill;
                default:
                    throw new ArgumentOutOfRangeException(nameof(skillType), skillType, null);
            }
        }
    }
}