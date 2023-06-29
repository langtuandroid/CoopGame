using System;
using System.Collections.Generic;
using Fusion;
using Main.Scripts.Utils;
using UnityEngine.Events;

namespace Main.Scripts.Player
{
    public class PlayersHolder : NetworkBehaviour
    {
        [Networked(OnChanged = nameof(OnPlayerObjectsChanged)), Capacity(16)]
        private NetworkDictionary<PlayerRef, NetworkObject> playerObjects => default;

        private Dictionary<int, PlayerController> cachedPlayerControllers = new();

        public UnityEvent OnChangedEvent = default!;

        public void Add(PlayerRef playerRef, PlayerController playerController)
        {
            if (playerObjects.ContainsKey(playerRef))
            {
                throw new Exception("Player controller is already cashed");
            }

            playerObjects.Add(playerRef, playerController.Object);
            cachedPlayerControllers.Add(playerRef, playerController);
        }

        public bool Contains(PlayerRef playerRef)
        {
            return playerObjects.ContainsKey(playerRef) && playerObjects[playerRef] != null;
        }

        public PlayerController Get(PlayerRef playerRef)
        {
            return Get(playerRef.PlayerId);
        }

        public PlayerController Get(int playerRef)
        {
            if (!cachedPlayerControllers.ContainsKey(playerRef))
            {
                var networkObject = playerObjects[playerRef].ThrowWhenNull();
                cachedPlayerControllers[playerRef] = networkObject.GetComponent<PlayerController>().ThrowWhenNull();
            }

            return cachedPlayerControllers[playerRef];
        }

        public void Remove(PlayerRef playerRef)
        {
            playerObjects.Remove(playerRef);
            cachedPlayerControllers.Remove(playerRef.PlayerId);
        }

        public List<PlayerRef> GetKeys(bool isValueContains = true)
        {
            var list = new List<PlayerRef>();
            foreach (var (playerRef, _) in playerObjects)
            {
                if (!isValueContains || Contains(playerRef))
                {
                    list.Add(playerRef);
                }
            }

            return list;
        }

        public static void OnPlayerObjectsChanged(Changed<PlayersHolder> changed)
        {
            if (changed.Behaviour)
            {
                changed.Behaviour.OnChangedEvent.Invoke();
            }
        }
    }
}