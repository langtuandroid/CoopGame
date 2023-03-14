using Fusion;
using UnityEngine.Events;

namespace Main.Scripts.Weapon
{
    public abstract class ActiveSkillBase : NetworkBehaviour
    {
        public abstract bool Activate(PlayerRef owner);

        public UnityEvent<ActiveSkillBase> OnSkillExecutedEvent = default!;

        public UnityEvent<ActiveSkillBase> OnSkillFinishedEvent = default!;
    }
}