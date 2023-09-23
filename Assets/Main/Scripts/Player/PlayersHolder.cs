using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using UnityEngine;

namespace Main.Scripts.Player
{
    public class PlayersHolder : MonoBehaviour
    {
        private Dictionary<PlayerRef, PlayerController> playerControllers = new();

        private List<Listener> listeners = new();

        public void Add(PlayerRef playerRef, PlayerController playerController)
        {
            if (playerControllers.ContainsKey(playerRef))
            {
                throw new Exception("Player controller is already cashed");
            }

            playerControllers.Add(playerRef, playerController);
            foreach (var listener in listeners)
            {
                listener.OnAdded(playerRef);
            }
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
            foreach (var listener in listeners)
            {
                listener.OnRemoved(playerRef);
            }
        }

        public List<PlayerRef> GetKeys()
        {
            //todo allocating
            return playerControllers.Keys.ToList();
        }

        public void AddListener(Listener listener)
        {
            listeners.Add(listener);
        }

        public void RemoveListener(Listener listener)
        {
            listeners.Remove(listener);
        }

        public interface Listener
        {
            void OnAdded(PlayerRef playerRef);
            void OnRemoved(PlayerRef playerRef);
        }
    }
}