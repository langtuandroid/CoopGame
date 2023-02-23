using Fusion;

namespace Main.Scripts.Player
{
    public class PlayersHolder : NetworkBehaviour
    {
        [Networked, Capacity(16)]
        public NetworkDictionary<PlayerRef, PlayerController> Players => default;
    }
}