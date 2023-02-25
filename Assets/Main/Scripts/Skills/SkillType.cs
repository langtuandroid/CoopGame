using System;

namespace Main.Scripts.Skills
{
    public enum SkillType
    {
        HEALTH_BOOST_PASSIVE,
        DAMAGE_BOOST_PASSIVE,
        SPEED_BOOST_PASSIVE
    }

    public static class SkillTypeExtensions
    {
        public static string GetKey(this SkillType skillType)
        {
            return skillType switch
            {
                SkillType.HEALTH_BOOST_PASSIVE => "health_boost_passive",
                SkillType.DAMAGE_BOOST_PASSIVE => "damage_boost_passive",
                SkillType.SPEED_BOOST_PASSIVE => "speed_boost_passive",
                _ => throw new ArgumentOutOfRangeException(nameof(skillType), skillType, null)
            };
        }
    }
}