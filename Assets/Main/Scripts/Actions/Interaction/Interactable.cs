using Fusion;

namespace Main.Scripts.Actions.Interaction
{
    public interface Interactable
    {
        bool IsInteractionEnabled(PlayerRef playerRef);
        void SetInteractionInfoVisibility(PlayerRef playerRef, bool isVisible);
        void AddInteract(PlayerRef playerRef);
    }
}