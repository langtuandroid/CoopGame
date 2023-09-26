using Fusion;

namespace Main.Scripts.Skills.Common.Component.Visual
{
    public class SkillNetworkTransform: NetworkTransform
    {
        private SkillComponent? skillComponent;

        public void FollowSkillComponent(SkillComponent skillComponent)
        {
            this.skillComponent = skillComponent;
            skillComponent.OnReadyToRelease += OnReadyToRelease;
        }

        private void OnReadyToRelease(SkillComponent skillComponent)
        {
            skillComponent.OnReadyToRelease -= OnReadyToRelease;
            this.skillComponent = null;
            
            Runner.Despawn(Object);
        }

        protected override void CopyFromEngineToBuffer()
        {
            if (skillComponent != null)
            {
                transform.position = skillComponent.Position;
                transform.rotation = skillComponent.Rotation;
            }

            base.CopyFromEngineToBuffer();
        }
    }
}