using Fusion;
using UnityEngine;

namespace Main.Scripts.Player
{
    public class PlayersHolder : NetworkBehaviour
    {
        [Networked, Capacity(16)]
        public NetworkDictionary<PlayerRef, PlayerController> players => default;
    }
}