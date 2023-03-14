using UnityEngine;
using UnityEngine.Events;

namespace Main.Scripts.Weapon
{
    public abstract class ActivePointSkillBase : ActiveSkillBase
    {
        public abstract void ApplyTargetPosition(Vector2 position);

        public abstract void Execute();

        public abstract void Cancel();

        public UnityEvent<ActivePointSkillBase> OnWaitingForPointEvent = default!;

        public UnityEvent<ActivePointSkillBase> OnSkillCanceledEvent = default!;
    }
}