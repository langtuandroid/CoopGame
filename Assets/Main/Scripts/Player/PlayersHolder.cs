using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using UnityEngine;
using UnityEngine.Events;

namespace Main.Scripts.Player
{
    public class PlayersHolder : MonoBehaviour
    {
        private Dictionary<PlayerRef, PlayerController> playerControllers = new();

        public UnityEvent OnChangedEvent = default!;

        public void Add(PlayerRef playerRef, PlayerController playerController)
        {
            if (playerControllers.ContainsKey(playerRef))
            {
                throw new Exception("Player controller is already cashed");
            }

            playerControllers.Add(playerRef, playerController);
            OnChangedEvent.Invoke();
        }

        public bool Contains(PlayerRef playerRef)
        {
            return playerControllers.ContainsKey(playerRef);
        }

        public PlayerController Get(PlayerRef playerRef)
        {
            return playerControllers[playerRef];
        }

        public void Remove(PlayerRef playerRef)
        {
            playerControllers.Remove(playerRef);
            OnChangedEvent.Invoke();
        }

        public List<PlayerRef> GetKeys()
        {
            //todo allocating
            return playerControllers.Keys.ToList();
        }
    }
}