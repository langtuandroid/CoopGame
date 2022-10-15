using System.Collections.Generic;
using Fusion;
using Main.Scripts.Player;
using UnityEngine;

namespace Main.Scripts
{
    public class PlayerManager : MonoBehaviour
    {
        private static List<PlayerController> _allPlayers = new List<PlayerController>();
        public static List<PlayerController> allPlayers => _allPlayers;

        private static Queue<PlayerController> _playerQueue = new Queue<PlayerController>();

        private static PlayerCamera _playerCamera;

        private static PlayerCamera PlayerCamera
        {
            get
            {
                if (_playerCamera == null)
                    _playerCamera = FindObjectOfType<PlayerCamera>(true);
                return _playerCamera;
            }
        }

        public static void HandleNewPlayers()
        {
            if (_playerQueue.Count > 0)
            {
                PlayerController player = _playerQueue.Dequeue();
                player.Respawn(0);
            }
        }

        public static int PlayersAlive()
        {
            var playersAlive = 0;
            for (var i = 0; i < _allPlayers.Count; i++)
            {
                if (_allPlayers[i].isActivated || allPlayers[i].state == PlayerController.State.Active)
                    playersAlive++;
            }

            return playersAlive;
        }

        public static PlayerController GetFirstAlivePlayer()
        {
            for (var i = 0; i < _allPlayers.Count; i++)
            {
                if (_allPlayers[i].isActivated)
                    return _allPlayers[i];
            }

            return null;
        }

        public static void AddPlayer(PlayerController player)
        {
            Debug.Log("Player Added");

            var insertIndex = _allPlayers.Count;
            // Sort the player list when adding players
            for (var i = 0; i < _allPlayers.Count; i++)
            {
                if (_allPlayers[i].playerID > player.playerID)
                {
                    insertIndex = i;
                    break;
                }
            }

            _allPlayers.Insert(insertIndex, player);
            _playerQueue.Enqueue(player);

            if (player.HasInputAuthority)
            {
                PlayerCamera.SetTarget(player);
            }
        }

        public static void RemovePlayer(PlayerController player)
        {
            if (player == null || !_allPlayers.Contains(player))
                return;

            Debug.Log("Player Removed " + player.playerID);

            _allPlayers.Remove(player);
            // if(CameraStrategy) // FindObject May return null on shutdown, so let's avoid that NPE
            // 	CameraStrategy.RemoveTarget(player.gameObject);
        }

        public static void ResetPlayerManager()
        {
            Debug.Log("Clearing Player Manager");
            allPlayers.Clear();
            // if(CameraStrategy) // FindObject May return null on shutdown, so let's avoid that NPE
            // 	CameraStrategy.RemoveAll();
        }

        public static PlayerController GetPlayerFromID(int id)
        {
            foreach (PlayerController player in _allPlayers)
            {
                if (player.playerID == id)
                    return player;
            }

            return null;
        }

        public static PlayerController Get(PlayerRef playerRef)
        {
            for (int i = _allPlayers.Count - 1; i >= 0; i--)
            {
                if (_allPlayers[i] == null || _allPlayers[i].Object == null)
                {
                    _allPlayers.RemoveAt(i);
                    Debug.Log("Removing null player");
                }
                else if (_allPlayers[i].Object.InputAuthority == playerRef)
                    return _allPlayers[i];
            }

            return null;
        }
    }
}