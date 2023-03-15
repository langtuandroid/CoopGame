using System;
using Fusion;
using UnityEngine;

namespace Main.Scripts.Skills
{
    public class SkillInfoHolder : MonoBehaviour
    {
        public static SkillInfoHolder? Instance { get; private set; }
        
        [SerializeField]
        private SkillInfo healthBoostInfo = default!;
        [SerializeField]
        private SkillInfo damageBoostInfo = default!;
        [SerializeField]
        private SkillInfo speedBoostInfo = default!;

        private void Awake()
        {
            Assert.Check(Instance == null);
            Instance = this;
        }

        private void OnDestroy()
        {
            Instance = null;
        }

        public SkillInfo GetSkillInfo(SkillType skillType)
        {
            return skillType switch
            {
                SkillType.HEALTH_BOOST_PASSIVE => healthBoostInfo,
                SkillType.DAMAGE_BOOST_PASSIVE => damageBoostInfo,
                SkillType.SPEED_BOOST_PASSIVE => speedBoostInfo,
                _ => throw new ArgumentOutOfRangeException(nameof(skillType), skillType, "Skill is not allowed")
            };
        }
    }
}