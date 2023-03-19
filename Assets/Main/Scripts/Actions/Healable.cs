namespace Main.Scripts.Actions
{
    public interface Healable : HealthProvider
    {
        void ApplyHeal(uint healValue);
    }
}