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
        public SkillControllerConfig? PrimarySkillConfig;
        public SkillControllerConfig? DashSkillConfig;
        public SkillControllerConfig? FirstSkillConfig;
        public SkillControllerConfig? SecondSkillConfig;
        public SkillControllerConfig? ThirdSkillConfig;
    }
}