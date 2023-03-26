using Fusion;
using UnityEngine;
using UnityEngine.Events;

namespace Main.Scripts.Skills.ActiveSkills
{
    public abstract class ActiveSkillBase : NetworkBehaviour
    {
        public abstract bool Activate(PlayerRef owner);

        public abstract bool IsOverrideMove();
        [HideInInspector]
        public UnityEvent<ActiveSkillBase> OnSkillExecutedEvent = default!;
        [HideInInspector]
        public UnityEvent<ActiveSkillBase> OnSkillFinishedEvent = default!;
    }
}