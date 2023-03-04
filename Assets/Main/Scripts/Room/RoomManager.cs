using System.Collections.Generic;
using Fusion;
using Main.Scripts.Levels.Results;
using Main.Scripts.Player.Data;
using Main.Scripts.Room.Level;
using Main.Scripts.Room.Scene;
using Main.Scripts.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace Main.Scripts.Room
{
    public class RoomManager : NetworkBehaviour
    {
        

        public const ShutdownReason ShutdownReason_GameAlreadyRunning = (ShutdownReason)100;

        private ConnectionManager connectionManager = default!;
        private LevelTransitionManager levelTransitionManager = default!;
        private PlayerDataManager playerDataManager = default!;

        private bool isGameAlreadyRunning;

        [Networked]
        private SceneState sceneState { get; set; }
        [Networked, Capacity(16)]
        private NetworkDictionary<UserId, PlayerRef> playerRefsMap => default;
        [Networked, Capacity(16)]
        private NetworkDictionary<PlayerRef, UserId> userIdsMap => default;
        [Networked, Capacity(16)]
        private NetworkDictionary<UserId, PlayerData> playersDataMap => default;
        [Networked, Capacity(16)]
        private NetworkDictionary<UserId, LevelResultsData> levelResults => default;

        public UnityEvent<PlayerRef> OnPlayerInitializedEvent = default!;
        public UnityEvent<PlayerRef> OnPlayerDisconnectedEvent = default!;

        public void Awake()
        {
            connectionManager = FindObjectOfType<ConnectionManager>(true).ThrowWhenNull();
            levelTransitionManager = FindObjectOfType<LevelTransitionManager>(true).ThrowWhenNull();
            playerDataManager = FindObjectOfType<PlayerDataManager>().ThrowWhenNull();
        }

        public override void Spawned()
        {
            DontDestroyOnLoad(this);

            InitPlayerData();

            if (Object.HasStateAuthority)
            {
                levelTransitionManager.OnSceneStateChangedEvent.AddListener(OnSceneStateChanged);
                connectionManager.OnPlayerConnectEvent.AddListener(OnPlayerConnect);
                connectionManager.OnPlayerDisconnectEvent.AddListener(OnPlayerDisconnect);
                playerDataManager.OnPlayerDataChangedEvent.AddListener(OnPlayerDataChanged);

                LoadLevel(-1);
            }
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            levelTransitionManager.OnSceneStateChangedEvent.RemoveListener(OnSceneStateChanged);
            connectionManager.OnPlayerConnectEvent.RemoveListener(OnPlayerConnect);
            connectionManager.OnPlayerDisconnectEvent.RemoveListener(OnPlayerDisconnect);
            playerDataManager.OnPlayerDataChangedEvent.RemoveListener(OnPlayerDataChanged);
        }

        private void OnPlayerConnect(NetworkRunner runner, PlayerRef playerRef)
        {
            //todo sync in level controller
        }

        private void OnPlayerDisconnect(NetworkRunner runner, PlayerRef playerRef)
        {
            OnPlayerDisconnectedEvent.Invoke(playerRef);
            if (userIdsMap.ContainsKey(playerRef))
            {
                userIdsMap.Remove(playerRef, out var userId);
                playerRefsMap.Remove(userId);
                if (sceneState != SceneState.LEVEL)
                {
                    playersDataMap.Remove(userId);
                    levelResults.Remove(userId);
                }
            }
        }

        private void InitPlayerData()
        {
            RPC_InitPlayerData(
                playerRef: Runner.LocalPlayer,
                userId: connectionManager.CurrentUserId,
                playerData: playerDataManager.LocalPlayerData
            );
        }

        public IEnumerable<PlayerRef> GetConnectedPlayers()
        {
            //copy for safe outside use
            return Runner.ActivePlayers;
        }

        public bool IsPlayerInitialized(PlayerRef playerRef)
        {
            return userIdsMap.ContainsKey(playerRef);
        }

        public UserId GetUserId(PlayerRef playerRef)
        {
            return userIdsMap.Get(playerRef);
        }

        public PlayerData GetPlayerData(UserId userId)
        {
            return playersDataMap.Get(userId);
        }

        public void OnPlayerDataChanged(UserId userId, PlayerData playerData)
        {
            playersDataMap.Set(userId, playerData);
        }

        public LevelResultsData? GetLevelResults(UserId userId)
        {
            if (levelResults.TryGet(userId, out var levelResultsData))
            {
                return levelResultsData;
            }

            return null;
        }

        public void OnLevelResultsShown(UserId userId)
        {
            levelResults.Remove(userId);
        }

        private void ShutdownRoomConnection(ShutdownReason shutdownReason)
        {
            if (!Runner.IsShutdown)
            {
                // Calling with destroyGameObject false because we do this in the OnShutdown callback on FusionLauncher
                Runner.Shutdown(false, shutdownReason);
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Backspace))
            {
                ShutdownRoomConnection(isGameAlreadyRunning ? ShutdownReason_GameAlreadyRunning : ShutdownReason.Ok);
            }
        }

        public void OnAllPlayersReady()
        {
            LoadLevel(0);
        }

        public void OnLevelFinished(Dictionary<UserId, LevelResultsData> levelResults)
        {
            this.levelResults.Clear();
            foreach (var (userId, levelResultsData) in levelResults)
            {
                if (playerRefsMap.ContainsKey(userId))
                {
                    this.levelResults.Add(userId, levelResultsData);
                    playerDataManager.RPC_ApplyPlayerRewards(playerRefsMap.Get(userId), levelResultsData);
                }
            }

            LoadLevel(-1);
        }

        private void OnSceneStateChanged(SceneState sceneState)
        {
            if (HasStateAuthority)
            {
                this.sceneState = sceneState;
                foreach (var (userId, _) in playersDataMap)
                {
                    if (!playerRefsMap.ContainsKey(userId))
                    {
                        playersDataMap.Remove(userId);
                    }
                }
            }
        }

        private void LoadLevel(int nextLevelIndex)
        {
            if (!Object.HasStateAuthority)
                return;

            levelTransitionManager.LoadLevel(nextLevelIndex);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPC_InitPlayerData(PlayerRef playerRef, UserId userId, PlayerData playerData)
        {
            if (playersDataMap.ContainsKey(userId))
            {
                var cashedPlayerData = playersDataMap.Get(userId);
                if (!cashedPlayerData.Equals(playerData))
                {
                    Runner.Disconnect(playerRef);
                    return;
                }
            }

            playerRefsMap.Set(userId, playerRef);
            userIdsMap.Set(playerRef, userId);
            playersDataMap.Set(userId, playerData);

            OnPlayerInitializedEvent.Invoke(playerRef);
        }
    }
}