using Fusion;
using UnityEngine.Events;

namespace Main.Scripts.ActiveSkills
{
    public abstract class ActiveSkillBase : NetworkBehaviour
    {
        public abstract bool Activate(PlayerRef owner);

        public abstract bool IsOverrideMove();

        public UnityEvent<ActiveSkillBase> OnSkillExecutedEvent = default!;

        public UnityEvent<ActiveSkillBase> OnSkillFinishedEvent = default!;
    }
}