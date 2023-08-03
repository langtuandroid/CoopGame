using Main.Scripts.Actions.Data;

namespace Main.Scripts.Actions
{
    public interface ObjectWithGettingKnockBack
    {
        void AddKnockBack(ref KnockBackActionData data);
    }
}