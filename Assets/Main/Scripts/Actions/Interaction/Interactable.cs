using Fusion;

namespace Main.Scripts.Actions.Interaction
{
    public interface Interactable
    {
        bool IsInteractionEnabled(PlayerRef playerRef);
        void SetInteractionInfoVisibility(PlayerRef player, bool isVisible);
        bool Interact(PlayerRef playerRef);
    }
}