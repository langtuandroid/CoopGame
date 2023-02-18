using Fusion;

namespace Main.Scripts.Weapon
{
    public abstract class ActiveSkillBase : NetworkBehaviour
    {
        public abstract bool Activate(PlayerRef owner);
        public abstract bool IsRunning();
    }
}