using System;
using Fusion;
using JetBrains.Annotations;
using Main.Scripts.Enemies;
using Main.Scripts.Player;
using Main.Scripts.Room;
using UnityEngine;

namespace Main.Scripts.Levels.Lobby
{
    public class LobbyLevelController : NetworkBehaviour
    {
        [SerializeField]
        private PlayerController playerPrefab;
        [SerializeField]
        private PlayersHolder playersHolder;

        private ConnectionManager connectionManager;
        private RoomManager roomManager;

        private PlayerCamera playerCamera;

        public void Awake()
        {
            connectionManager = FindObjectOfType<ConnectionManager>();
            roomManager = FindObjectOfType<RoomManager>();
            playerCamera = PlayerCamera.Instance;
        }

        public override void Spawned()
        {
            if (HasStateAuthority)
            {
                var connectedPlayers = roomManager.GetConnectedPlayers();
                foreach (var playerRef in connectedPlayers)
                {
                    if (!playersHolder.players.ContainsKey(playerRef))
                    {
                        OnPlayerConnect(Runner, playerRef);
                    }
                }
                connectionManager.OnPlayerConnectEvent.AddListener(OnPlayerConnect);
            }
        }

        public override void Render()
        {
            if (playersHolder.players.ContainsKey(Runner.LocalPlayer))
            {
                playerCamera.SetTarget(playersHolder.players.Get(Runner.LocalPlayer).transform);
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
                position: Vector3.zero,
                rotation: Quaternion.identity,
                inputAuthority: playerRef,
                onBeforeSpawned: (networkRunner, playerObject) =>
                {
                    var playerController = playerObject.GetComponent<PlayerController>();
                    
                    playersHolder.players.Add(playerRef, playerController);
                    playerController.OnPlayerDeadEvent.AddListener(OnPlayerDead);
                }
            );
        }

        private void OnPlayerDead(PlayerRef playerRef)
        {
            //todo
        }
    }
}