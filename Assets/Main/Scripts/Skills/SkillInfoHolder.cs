using System;
using UnityEngine;

namespace Main.Scripts.Skills
{
    public class SkillInfoHolder : MonoBehaviour
    {
        [SerializeField]
        private SkillInfo healthBoostInfo;
        [SerializeField]
        private SkillInfo damageBoostInfo;
        [SerializeField]
        private SkillInfo speedBoostInfo;
        
        public SkillInfo GetSkillInfo(SkillType skillType)
        {
            switch (skillType)
            {
                case SkillType.HEALTH_BOOST_PASSIVE:
                    return healthBoostInfo;
                case SkillType.DAMAGE_BOOST_PASSIVE:
                    return damageBoostInfo;
                case SkillType.SPEED_BOOST_PASSIVE:
                    return speedBoostInfo;
                default:
                    throw new ArgumentOutOfRangeException(nameof(skillType), skillType, "Skill is not allowed");
            }
        }
    }
}