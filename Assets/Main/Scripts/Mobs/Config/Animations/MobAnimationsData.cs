using System;
using System.Collections.Generic;
using FSG.MeshAnimator.ShaderAnimated;

namespace Main.Scripts.Mobs.Config.Animations
{
    [Serializable]
    public struct MobAnimationsData
    {
        public ShaderMeshAnimation? IdleAnimation;
        public ShaderMeshAnimation? RunAnimation;
        public List<MobSkillAnimationData> SkillAnimationsData;
    }
}