using System;
using System.Linq;
using Fusion;
using Main.Scripts.Player;
using Main.Scripts.Player.Experience;
using Main.Scripts.Room;
using Main.Scripts.Skills;
using Main.Scripts.Tasks;
using Main.Scripts.UI.Windows;
using UnityEngine;

namespace Main.Scripts.Levels.Lobby
{
    public class LobbyLevelController : NetworkBehaviour
    {
        [SerializeField]
        private PlayerController playerPrefab;
        [SerializeField]
        private PlayersHolder playersHolder;
        [SerializeField]
        private PlaceTargetTask readyToStartTask;

        private ConnectionManager connectionManager;
        private RoomManager roomManager;

        private PlayerCamera playerCamera;

        public void Awake()
        {
            connectionManager = FindObjectOfType<ConnectionManager>();
            roomManager = FindObjectOfType<RoomManager>();
            playerCamera = FindObjectOfType<PlayerCamera>();
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
                readyToStartTask.OnTaskCompleted.AddListener(OnAllPlayersReady);
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
                playersHolder.players.Get(playerRef).GetComponent<WindowsController>().SetCurrentWindowType(WindowType.LEVEL_RESULTS);
            }
        }
    }
}