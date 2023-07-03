using System;
using Main.Scripts.Skills.Common.Controller;
using UnityEngine;

namespace Main.Scripts.Skills.ActiveSkills
{
    [Serializable]
    public struct ActiveSkillsConfig
    {
        public LayerMask AlliesLayerMask;
        public LayerMask OpponentsLayerMask;
        public SkillController? PrimarySkill;
        public SkillController? DashSkill;
        public SkillController? FirstSkill;
        public SkillController? SecondSkill;
        public SkillController? ThirdSkill;
    }
}