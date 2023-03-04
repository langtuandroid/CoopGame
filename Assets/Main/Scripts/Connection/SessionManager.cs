using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using Main.Scripts.Player.Data;
using Main.Scripts.Room;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Main.Scripts.Connection
{
    public class SessionManager : MonoBehaviour, INetworkRunnerCallbacks
    {
        [SerializeField]
        private RoomManager roomManagerPrefab = default!;
        [SerializeField]
        private PlayerDataManager playerDataManagerPrefab = default!;

        private NetworkRunner? runner;
        private FusionObjectPoolRoot? pool;

        public UserId LocalUserId { get; private set; }
        public ConnectionStatus CurrentConnectionStatus { get; private set; }

        [FormerlySerializedAs("OnConnectionStatusChanged")]
        public UnityEvent<ConnectionStatus> OnConnectionStatusChangedEvent = default!;
        public UnityEvent<PlayerRef> OnPlayerConnectedEvent = default!;
        public UnityEvent<PlayerRef> OnPlayerDisconnectedEvent = default!;

        public async void LaunchSession(
            GameMode mode,
            string room,
            UserId userId,
            INetworkSceneManager sceneManager
        )
        {
            if (CurrentConnectionStatus is ConnectionStatus.Connecting or ConnectionStatus.Connected) return;

            SetConnectionStatus(ConnectionStatus.Connecting);

            if (runner == null)
            {
                runner = gameObject.AddComponent<NetworkRunner>();
            }

            runner.name = name;
            runner.ProvideInput = mode != GameMode.Server;

            if (pool == null)
            {
                pool = gameObject.AddComponent<FusionObjectPoolRoot>();
            }

            LocalUserId = userId;

            await runner.StartGame(new StartGameArgs
            {
                GameMode = mode,
                SessionName = room,
                ObjectPool = pool,
                SceneManager = sceneManager
            });
        }

        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
        {
            request.Accept();
        }

        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
        {
            Debug.Log($"Connect failed {reason}");
            SetConnectionStatus(ConnectionStatus.Failed, reason.ToString());
        }

        public void OnConnectedToServer(NetworkRunner runner)
        {
            Debug.Log("Client connected to server");
        }

        public void OnDisconnectedFromServer(NetworkRunner runner)
        {
            Debug.Log("Disconnected from server");
            SetConnectionStatus(ConnectionStatus.Disconnected, "Disconnected from server");
        }

        public void OnSceneLoadStart(NetworkRunner runner) { }

        public void OnSceneLoadDone(NetworkRunner runner) { }

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            Debug.Log("OnShutdown");
            var message = shutdownReason switch
            {
                RoomManager.ShutdownReason_GameAlreadyRunning => "Game in this room already started!",
                ShutdownReason.IncompatibleConfiguration => "This room already exist in a different game mode!",
                ShutdownReason.Ok => "User terminated network session!",
                ShutdownReason.Error => "Unknown network error!",
                ShutdownReason.ServerInRoom => "There is already a server/host in this room",
                ShutdownReason.DisconnectedByPluginLogic => "The Photon server plugin terminated the network session!",
                _ => shutdownReason.ToString()
            };

            SetConnectionStatus(ConnectionStatus.Disconnected, message);
        }

        private void Release()
        {
            OnConnectionStatusChangedEvent.RemoveAllListeners();
            OnPlayerConnectedEvent.RemoveAllListeners();
            OnPlayerDisconnectedEvent.RemoveAllListeners();

            // This cleanup should be handled by the ClearPools call below, but currently Fusion is not returning pooled objects on shutdown, so...
            // Destroy all NOs
            var nos = FindObjectsOfType<NetworkObject>();
            for (var i = 0; i < nos.Length; i++)
            {
                Destroy(nos[i].gameObject);
            }

            // Reset the object pools
            if (pool != null)
            {
                pool.ClearPools();
            }

            if (runner != null)
            {
                Destroy(runner.gameObject);
            }
        }

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef playerRef)
        {
            Debug.Log($"OnPlayerJoined {playerRef}");
            if (playerRef == runner.LocalPlayer)
            {
                SetConnectionStatus(ConnectionStatus.Connected);

                if (runner.IsServer)
                {
                    InitServer(runner);
                }
            }

            OnPlayerConnectedEvent.Invoke(playerRef);
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef playerRef)
        {
            Debug.Log($"Player left {playerRef}");
            OnPlayerDisconnectedEvent.Invoke(playerRef);
        }

        public void OnInput(NetworkRunner runner, NetworkInput input) { }
        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }

        private void SetConnectionStatus(ConnectionStatus status, string message = "")
        {
            Debug.Log($"{status}, from {CurrentConnectionStatus}, message: {message}");
            if (CurrentConnectionStatus == status) return;

            CurrentConnectionStatus = status;
            OnConnectionStatusChangedEvent.Invoke(status);

            if (CurrentConnectionStatus is ConnectionStatus.Disconnected or ConnectionStatus.Failed)
            {
                Release();
            }
        }

        /*
         * Run on Server side when Server is connected
         */
        private void InitServer(NetworkRunner runner)
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
    }
}