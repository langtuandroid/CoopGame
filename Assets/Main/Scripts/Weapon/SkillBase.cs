using Fusion;

namespace Main.Scripts.Weapon
{
    public abstract class SkillBase : NetworkBehaviour
    {
        public abstract bool Activate(PlayerRef owner);
        public abstract bool IsRunning();
    }
}