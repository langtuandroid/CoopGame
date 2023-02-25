using System;
using System.Collections.Generic;
using Fusion;
using Main.Scripts.Utils;

namespace Main.Scripts.Player
{
    public class PlayersHolder : NetworkBehaviour
    {
        [Networked, Capacity(16)]
        private NetworkDictionary<PlayerRef, NetworkObject> playerObjects => default;

        private Dictionary<PlayerRef, PlayerController> cashedPlayerControllers = new();

        public void Add(PlayerRef playerRef, PlayerController playerController)
        {
            if (playerObjects.ContainsKey(playerRef))
            {
                throw new Exception("Player controller is already cashed");
            }

            playerObjects.Add(playerRef, playerController.Object);
            cashedPlayerControllers.Add(playerRef, playerController);
        }

        public bool Contains(PlayerRef playerRef)
        {
            return playerObjects.ContainsKey(playerRef) && playerObjects[playerRef] != null;
        }

        public PlayerController Get(PlayerRef playerRef)
        {
            if (!cashedPlayerControllers.ContainsKey(playerRef))
            {
                var networkObject = playerObjects.Get(playerRef).ThrowWhenNull();
                cashedPlayerControllers[playerRef] = networkObject.GetComponent<PlayerController>().ThrowWhenNull();
            }

            return cashedPlayerControllers[playerRef];
        }

        public void Remove(PlayerRef playerRef)
        {
            playerObjects.Remove(playerRef);
            cashedPlayerControllers.Remove(playerRef);
        }

        public IEnumerable<PlayerRef> GetKeys()
        {
            var list = new List<PlayerRef>();
            foreach (var (playerRef, _) in playerObjects)
            {
                list.Add(playerRef);
            }

            return list;
        }
    }
}