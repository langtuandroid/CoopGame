using Main.Scripts.Actions.Data;

namespace Main.Scripts.Actions
{
    public interface Dashable
    {
        void AddDash(ref DashActionData data);
    }
}