using System;
using Fusion;
using Main.Scripts.Player;
using Main.Scripts.Room;
using UnityEngine;

namespace Main.Scripts.Levels.Lobby
{
    public class LobbyLevelController : NetworkBehaviour
    {
        [SerializeField]
        private PlayerController playerPrefab;

        private ConnectionManager connectionManager;
        private RoomManager roomManager;

        [Networked]
        private NetworkDictionary<PlayerRef, PlayerController> players => default;
        
        private PlayerCamera playerCamera;

        public void Awake()
        {
            connectionManager = FindObjectOfType<ConnectionManager>();
            roomManager = FindObjectOfType<RoomManager>();
            playerCamera = FindObjectOfType<PlayerCamera>(true);
        }

        public override void Spawned()
        {
            if (HasStateAuthority)
            {
                var connectedPlayers = roomManager.GetConnectedPlayers();
                foreach (var playerRef in connectedPlayers)
                {
                    if (!players.ContainsKey(playerRef))
                    {
                        OnPlayerConnect(Runner, playerRef);
                    }
                }
                connectionManager.OnPlayerConnectEvent.AddListener(OnPlayerConnect);
            }
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            connectionManager.OnPlayerConnectEvent.RemoveListener(OnPlayerConnect);
        }

        private void OnPlayerConnect(NetworkRunner runner, PlayerRef playerRef)
        {
            //todo добавить спавн поинты
            Runner.Spawn(
                prefab: playerPrefab,
                position: Vector3.up,
                rotation: Quaternion.identity,
                inputAuthority: playerRef,
                onBeforeSpawned: (networkRunner, playerObject) =>
                {
                    var playerController = playerObject.GetComponent<PlayerController>();
                    
                    players.Add(playerRef, playerController);
                    playerController.OnPlayerDeadEvent.AddListener(OnPlayerDead);

                    if (playerController.HasInputAuthority)
                    {
                        playerCamera.SetTarget(playerController);
                    }
                }
            );
        }

        private void OnPlayerDead(PlayerRef playerRef)
        {
            //todo
        }
    }
}