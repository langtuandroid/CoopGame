using Main.Scripts.Actions.Data;

namespace Main.Scripts.Actions
{
    public interface ObjectWithGettingStun
    {
        void AddStun(ref StunActionData data);
    }
}