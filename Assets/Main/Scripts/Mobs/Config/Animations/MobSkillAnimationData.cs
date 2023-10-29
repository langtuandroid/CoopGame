using System;
using System.Collections.Generic;
using FSG.MeshAnimator.ShaderAnimated;
using Main.Scripts.Skills.ActiveSkills;

namespace Main.Scripts.Mobs.Config.Animations
{
    [Serializable]
    public struct MobSkillAnimationData
    {
        public ActiveSkillType SkillType;
        public List<ShaderMeshAnimation> CastSkillAnimationsList;
        public List<ShaderMeshAnimation> ExecutionSkillAnimationsList;
    }
}