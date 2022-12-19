using Main.Scripts.Drop;

namespace Main.Scripts.Actions
{
    public interface ObjectWithPickUp
    {
        void OnPickUp(DropType dropType);
    }
}