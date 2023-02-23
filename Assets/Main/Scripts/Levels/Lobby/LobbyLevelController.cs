using System;
using Fusion;
using Main.Scripts.Player;
using Main.Scripts.Room;
using Main.Scripts.Tasks;
using Main.Scripts.UI.Windows;
using Main.Scripts.Utils;
using UnityEngine;

namespace Main.Scripts.Levels.Lobby
{
    public class LobbyLevelController : NetworkBehaviour
    {
        [SerializeField]
        private PlayerController playerPrefab = default!;
        [SerializeField]
        private PlayersHolder playersHolder = default!;
        [SerializeField]
        private PlaceTargetTask readyToStartTask = default!;

        private Lazy<ConnectionManager> connectionManagerLazy = new(
            () => FindObjectOfType<ConnectionManager>().ThrowWhenNull()
        );
        private Lazy<RoomManager> roomManagerLazy = new(
            () => FindObjectOfType<RoomManager>().ThrowWhenNull()
        );
        private Lazy<PlayerCamera> playerCameraLazy = new(
            () => FindObjectOfType<PlayerCamera>().ThrowWhenNull()
        );
        
        private ConnectionManager connectionManager => connectionManagerLazy.Value;
        private RoomManager roomManager => roomManagerLazy.Value;
        private PlayerCamera playerCamera => playerCameraLazy.Value;

        public override void Spawned()
        {
            if (HasStateAuthority)
            {
                var connectedPlayers = roomManager.GetConnectedPlayers();
                foreach (var playerRef in connectedPlayers)
                {
                    if (!playersHolder.Players.ContainsKey(playerRef))
                    {
                        OnPlayerConnect(Runner, playerRef);
                    }
                }

                connectionManager.OnPlayerConnectEvent.AddListener(OnPlayerConnect);
                readyToStartTask.OnTaskCompleted.AddListener(OnAllPlayersReady);
            }
        }

        public override void Render()
        {
            if (playersHolder.Players.ContainsKey(Runner.LocalPlayer))
            {
                playerCamera.SetTarget(playersHolder.Players.Get(Runner.LocalPlayer).transform);
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

                    playersHolder.Players.Add(playerRef, playerController);
                    playerController.OnPlayerDeadEvent.AddListener(OnPlayerDead);
                    playerController.OnPlayerStateChangedEvent.AddListener(OnPlayerStateChanged);
                }
            );
        }

        private void OnPlayerDead(PlayerRef playerRef)
        {
            //todo
        }

        private void OnPlayerStateChanged(PlayerRef playerRef, PlayerController.State playerState)
        {
            if (playerState == PlayerController.State.Active)
            {
                TryShowLevelResults(playerRef);
            }
        }

        private void OnAllPlayersReady()
        {
            roomManager.OnAllPlayersReady();
        }

        private void TryShowLevelResults(PlayerRef playerRef)
        {
            if (roomManager.GetLevelResults(playerRef) != null)
            {
                playersHolder.Players.Get(playerRef).GetComponent<WindowsController>().SetCurrentWindowType(WindowType.LEVEL_RESULTS);
            }
        }
    }
}