using Main.Scripts.Actions.Data;

namespace Main.Scripts.Actions.Health
{
    public interface Healable : HealthProvider
    {
        void AddHeal(ref HealActionData data);
    }
}