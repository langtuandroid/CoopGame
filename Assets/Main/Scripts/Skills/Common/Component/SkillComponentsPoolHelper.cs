using System.Collections.Generic;
using UnityEngine.Pool;

namespace Main.Scripts.Skills.Common.Component
{
    public class SkillComponentsPoolHelper
    {
        private HashSet<SkillComponent> activeSkillComponents = new();

        public SkillComponent Get()
        {
			var skillComponent = GenericPool<SkillComponent>.Get();
            activeSkillComponents.Add(skillComponent);
            skillComponent.OnReadyToRelease += OnReadyToRelease;
			return skillComponent;
        }

        public void Clear()
        {
            foreach (var skillComponent in activeSkillComponents)
            {
                skillComponent.OnReadyToRelease -= OnReadyToRelease;
                skillComponent.Release();
                GenericPool<SkillComponent>.Release(skillComponent);
            }
            activeSkillComponents.Clear();
        }

        private void OnReadyToRelease(SkillComponent skillComponent)
        {
            activeSkillComponents.Remove(skillComponent);
            skillComponent.OnReadyToRelease -= OnReadyToRelease;
            skillComponent.Release();
            GenericPool<SkillComponent>.Release(skillComponent);
        }
    }
}