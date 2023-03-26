using UnityEngine;
using UnityEngine.Events;

namespace Main.Scripts.Skills.ActiveSkills
{
    public abstract class ActivePointSkillBase : ActiveSkillBase
    {
        public abstract void ApplyTargetPosition(Vector2 position);

        public abstract void Execute();

        public abstract void Cancel();

        [HideInInspector]
        public UnityEvent<ActivePointSkillBase> OnWaitingForPointEvent = default!;
        [HideInInspector]
        public UnityEvent<ActivePointSkillBase> OnSkillCanceledEvent = default!;
    }
}