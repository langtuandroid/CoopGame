using System;
using Fusion;
using Main.Scripts.FusionHelpers;
using Main.Scripts.Player.Data;
using Main.Scripts.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace Main.Scripts.Room
{
    public class ConnectionManager : MonoBehaviour
    {
        [SerializeField]
        private RoomManager roomManagerPrefab = default!;
        [SerializeField]
        private GameObject skillInfoHolderPrefab = default!;
        [SerializeField]
        private GameObject playerDataManagerPrefab = default!;

        public string RoomName { get; private set; } = "Room";
        public UserId CurrentUserId { get; private set; } = new("Player");

        private FusionLauncher.ConnectionStatus connectionStatus = FusionLauncher.ConnectionStatus.Disconnected;

        public UnityEvent<NetworkRunner, PlayerRef> OnPlayerConnectEvent = default!;
        public UnityEvent<NetworkRunner, PlayerRef> OnPlayerDisconnectEvent = default!;

        private void Start()
        {
            OnConnectionStatusUpdate(null, FusionLauncher.ConnectionStatus.Disconnected, "");
        }

        public void CreateServer(string roomName, UserId userId)
        {
            OnEnterRoom(GameMode.Host, roomName, userId);
        }

        public void ConnectClient(string roomName, UserId userId)
        {
            OnEnterRoom(GameMode.Client, roomName, userId);
        }

        private void OnEnterRoom(GameMode gameMode, string roomName, UserId userId)
        {
            RoomName = roomName;
            CurrentUserId = userId;

            var fusionLauncher = FindObjectOfType<FusionLauncher>();
            if (fusionLauncher == null)
            {
                fusionLauncher = new GameObject("FusionLauncher").AddComponent<FusionLauncher>();
            }

            var levelTransitionManager = FindObjectOfType<LevelTransitionManager>().ThrowWhenNull();
            levelTransitionManager.launcher = fusionLauncher;

            fusionLauncher.Launch(
                mode: gameMode,
                room: RoomName,
                sceneLoader: levelTransitionManager,
                onConnect: OnConnectionStatusUpdate,
                onSpawnWorld: OnSpawnWorld,
                onSpawnPlayer: OnSpawnPlayer,
                onDespawnPlayer: OnDespawnPlayer
            );
        }

        private void OnConnectionStatusUpdate(
            NetworkRunner? runner,
            FusionLauncher.ConnectionStatus status,
            string reason
        )
        {
            Debug.Log(status);
            connectionStatus = status;

            switch (status)
            {
                case FusionLauncher.ConnectionStatus.Connected:
                    Debug.Log("Instantiate SkillInfoHolder");
                    Instantiate(skillInfoHolderPrefab);
                    break;
                case FusionLauncher.ConnectionStatus.Connecting:
                case FusionLauncher.ConnectionStatus.Loaded:
                case FusionLauncher.ConnectionStatus.Loading:
                    break;
                case FusionLauncher.ConnectionStatus.Disconnected:
                case FusionLauncher.ConnectionStatus.Failed:
                    //todo observer
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(status), status, null);
            }
        }

        private void OnSpawnWorld(NetworkRunner runner)
        {
            Debug.Log("Spawning PlayerDataManager");
            runner.Spawn(
                prefab: playerDataManagerPrefab,
                position: Vector3.zero,
                rotation: Quaternion.identity,
                inputAuthority: null
            );

            Debug.Log("Spawning RoomManager");
            runner.Spawn(
                prefab: roomManagerPrefab,
                position: Vector3.zero,
                rotation: Quaternion.identity,
                inputAuthority: null
            );
        }

        private void OnSpawnPlayer(NetworkRunner runner, PlayerRef playerRef)
        {
            Debug.Log($"Spawning Player {playerRef}");
            OnPlayerConnectEvent.Invoke(runner, playerRef);
        }

        private void OnDespawnPlayer(NetworkRunner runner, PlayerRef playerRef)
        {
            Debug.Log($"Despawning Player {playerRef}");
            OnPlayerDisconnectEvent.Invoke(runner, playerRef);
        }
    }
}