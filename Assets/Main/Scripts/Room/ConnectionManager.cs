using Fusion;
using Main.Scripts.FusionHelpers;
using UnityEngine;
using UnityEngine.Events;

namespace Main.Scripts.Room
{

    public class ConnectionManager : MonoBehaviour
    {
        [SerializeField]
        private RoomManager roomManagerPrefab;
        [SerializeField]
        private string roomName = "RoomName";

        private FusionLauncher.ConnectionStatus connectionStatus = FusionLauncher.ConnectionStatus.Disconnected;

        public UnityEvent<NetworkRunner, PlayerRef> OnPlayerConnectEvent;
        public UnityEvent<NetworkRunner, PlayerRef> OnPlayerDisconnectEvent;

        private void Awake()
        {
            DontDestroyOnLoad(this);
        }

        private void Start()
        {
            OnConnectionStatusUpdate(null, FusionLauncher.ConnectionStatus.Disconnected, "");
        }

        public void OnGUI()
        {
            //todo сделать нормальную менюшку

            switch (connectionStatus)
            {
                case FusionLauncher.ConnectionStatus.Connected:
                case FusionLauncher.ConnectionStatus.Connecting:
                case FusionLauncher.ConnectionStatus.Loaded:
                case FusionLauncher.ConnectionStatus.Loading:
                    return;
                case FusionLauncher.ConnectionStatus.Disconnected:
                case FusionLauncher.ConnectionStatus.Failed:
                    break;
            }

            if (GUI.Button(new Rect(10, 10, Screen.width - 20, Screen.height / 2f - 20), "Server"))
            {
                OnEnterRoom(GameMode.Host);
            }

            if (GUI.Button(new Rect(10, Screen.height / 2f + 10, Screen.width - 20, Screen.height / 2 - 20), "Client"))
            {
                OnEnterRoom(GameMode.Client);
            }
        }

        private void OnEnterRoom(GameMode gameMode)
        {
            var fusionLauncher = FindObjectOfType<FusionLauncher>();
            if (fusionLauncher == null)
            {
                fusionLauncher = new GameObject("FusionLauncher").AddComponent<FusionLauncher>();
            }

            var levelTransitionManager = FindObjectOfType<LevelTransitionManager>();
            levelTransitionManager.launcher = fusionLauncher;

            fusionLauncher.Launch(
                mode: gameMode,
                room: roomName,
                sceneLoader: levelTransitionManager,
                onConnect: OnConnectionStatusUpdate,
                onSpawnWorld: OnSpawnWorld,
                onSpawnPlayer: OnSpawnPlayer,
                onDespawnPlayer: OnDespawnPlayer
            );
        }

        private void OnConnectionStatusUpdate(
            NetworkRunner runner,
            FusionLauncher.ConnectionStatus status,
            string reason
        )
        {
            Debug.Log(status);
            connectionStatus = status;
            
            switch (status)
            {
                case FusionLauncher.ConnectionStatus.Connected:
                case FusionLauncher.ConnectionStatus.Connecting:
                case FusionLauncher.ConnectionStatus.Loaded:
                case FusionLauncher.ConnectionStatus.Loading:
                    break;
                case FusionLauncher.ConnectionStatus.Disconnected:
                case FusionLauncher.ConnectionStatus.Failed:
                    //todo observer
                    break;
            }
        }

        private void OnSpawnWorld(NetworkRunner runner)
        {
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