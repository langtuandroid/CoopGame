using System.Collections.Generic;
using Fusion;
using Main.Scripts.Connection;
using Main.Scripts.Levels.Results;
using Main.Scripts.Player.Data;
using Main.Scripts.Room.Transition;
using Main.Scripts.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace Main.Scripts.Room
{
    public class RoomManager : NetworkBehaviour, IAfterSpawned
    {
        public const ShutdownReason ShutdownReason_GameAlreadyRunning = (ShutdownReason)100;
        
        public static RoomManager? Instance { get; private set; }

        private SessionManager sessionManager = default!;
        private LevelTransitionManager levelTransitionManager = default!;
        private PlayerDataManager playerDataManager = default!;

        private bool isGameAlreadyRunning;

        [Networked, Capacity(4)]
        private NetworkDictionary<UserId, LevelResultsData> levelResults => default;

        public UnityEvent<PlayerRef> OnPlayerInitializedEvent = default!;
        public UnityEvent<PlayerRef> OnPlayerDisconnectedEvent = default!;

        public void Awake()
        {
            Assert.Check(Instance == null);
            Instance = this;
        }

        private void OnDestroy()
        {
            Instance = null;
        }

        public override void Spawned()
        {
            Debug.Log("RoomManager is spawned");
            DontDestroyOnLoad(this);
            
            sessionManager = SessionManager.Instance.ThrowWhenNull();
            levelTransitionManager = LevelTransitionManager.Instance.ThrowWhenNull();
            
            levelTransitionManager.OnSceneStateChangedEvent.AddListener(OnSceneStateChanged);
            
            sessionManager.OnPlayerConnectedEvent.AddListener(OnPlayerConnected);
            sessionManager.OnPlayerDisconnectedEvent.AddListener(OnPlayerDisconnected);
        }

        public void AfterSpawned()
        {
            playerDataManager = PlayerDataManager.Instance.ThrowWhenNull();
            playerDataManager.OnLocalPlayerDataReadyEvent.AddListener(OnLocalPlayerDataReady);

            if (Object.HasStateAuthority)
            {
                LoadLevel(-1);
            }
            
            if (playerDataManager.IsReady())
            {
                OnLocalPlayerDataReady();
            }
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            levelTransitionManager.OnSceneStateChangedEvent.RemoveListener(OnSceneStateChanged);
            sessionManager.OnPlayerConnectedEvent.RemoveListener(OnPlayerConnected);
            sessionManager.OnPlayerDisconnectedEvent.RemoveListener(OnPlayerDisconnected);
            playerDataManager.OnLocalPlayerDataReadyEvent.RemoveListener(OnLocalPlayerDataReady);
        }

        private void OnPlayerConnected(PlayerRef playerRef)
        {
            if (!HasStateAuthority) return;
            
            //todo sync in level controller
        }

        private void OnPlayerDisconnected(PlayerRef playerRef)
        {
            OnPlayerDisconnectedEvent.Invoke(playerRef);
            if (playerDataManager.HasPlayer(playerRef))
            {
                var clearPlayerData = levelTransitionManager.CurrentSceneState != SceneState.LEVEL;
                var userId = playerDataManager.GetUserId(playerRef);
                
                playerDataManager.RemovePlayer(playerRef, clearPlayerData);
                if (clearPlayerData)
                {
                    levelResults.Remove(userId);
                }
            }
        }

        private void OnLocalPlayerDataReady()
        {
            RPC_OnPlayerDataReady(
                playerRef: Runner.LocalPlayer,
                userId: playerDataManager.LocalUserId,
                playerData: playerDataManager.LocalPlayerData
            );
        }

        public bool IsPlayerInitialized(PlayerRef playerRef)
        {
            return playerDataManager.HasPlayer(playerRef);
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
                if (playerDataManager.HasPlayer(userId))
                {
                    this.levelResults.Add(userId, levelResultsData);
                    playerDataManager.RPC_ApplyPlayerRewards(playerDataManager.GetPlayerRef(userId), levelResultsData);
                }
            }

            LoadLevel(-1);
        }

        private void OnSceneStateChanged(SceneState sceneState)
        {
            playerDataManager.ClearAllKeepedPlayerData();
        }

        private void LoadLevel(int nextLevelIndex)
        {
            if (!HasStateAuthority) return;

            levelTransitionManager.LoadLevel(nextLevelIndex);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPC_OnPlayerDataReady(PlayerRef playerRef, UserId userId, PlayerData playerData)
        {
            var keepedPlayerData = playerDataManager.GetPlayerData(userId);
            if (playerDataManager.HasPlayer(userId))
            {
                Debug.Log("Kick player by userId is in game");
                RPC_KickPlayer(playerRef);
                return;
            }
            
            if (keepedPlayerData != null)
            {
                if (!keepedPlayerData.Equals(playerData))
                {
                    Debug.Log("Kick player by player data is not equals");
                    RPC_KickPlayer(playerRef);
                    return;
                }
            }
            else if (levelTransitionManager.CurrentSceneState is SceneState.LEVEL)
            {
                Debug.Log("Kick player by mission is started");
                RPC_KickPlayer(playerRef);
                return;
            }
            
            //todo можно попробовать отправлять playerData сразу от локального клиента другим клиентам, а подтверждение от сервера получать отдельно
            playerDataManager.AddPlayerData(playerRef, ref userId, ref playerData);
            playerDataManager.SendAllPlayersDataToClient(playerRef);

            OnPlayerInitializedEvent.Invoke(playerRef);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_KickPlayer([RpcTarget] PlayerRef playerRef)
        {
            Debug.Log("RPC_KickPlayer");
            ShutdownRoomConnection(ShutdownReason.GameIsFull);
        }
    }
}