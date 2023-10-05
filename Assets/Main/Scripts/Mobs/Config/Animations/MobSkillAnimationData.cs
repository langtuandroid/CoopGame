using System;
using FSG.MeshAnimator.ShaderAnimated;
using Main.Scripts.Skills.ActiveSkills;

namespace Main.Scripts.Mobs.Config.Animations
{
    [Serializable]
    public struct MobSkillAnimationData
    {
        public ActiveSkillType SkillType;
        public ShaderMeshAnimation? CastSkillAnimation;
        public ShaderMeshAnimation? ExecutionSkillAnimation;
    }
}