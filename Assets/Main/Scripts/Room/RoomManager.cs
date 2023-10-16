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
        private NetworkDictionary<PlayerRef, LevelResultsData> levelResults => default;

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
            
            sessionManager.OnPlayerConnectedEvent.AddListener(OnPlayerConnected);
            sessionManager.OnPlayerDisconnectedEvent.AddListener(OnPlayerDisconnected);
        }

        public void AfterSpawned()
        {
            playerDataManager = PlayerDataManager.Instance.ThrowWhenNull();
            playerDataManager.OnLocalUserDataReadyEvent.AddListener(OnLocalUserDataReady);

            if (Object.HasStateAuthority)
            {
                LoadLevel(-1);
            }
            
            if (playerDataManager.IsReady())
            {
                OnLocalUserDataReady();
            }
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            sessionManager.OnPlayerConnectedEvent.RemoveListener(OnPlayerConnected);
            sessionManager.OnPlayerDisconnectedEvent.RemoveListener(OnPlayerDisconnected);
            playerDataManager.OnLocalUserDataReadyEvent.RemoveListener(OnLocalUserDataReady);
        }

        private void OnPlayerConnected(PlayerRef playerRef)
        {
            if (!HasStateAuthority) return;
            
            //todo sync in level controller
        }

        private void OnPlayerDisconnected(PlayerRef playerRef)
        {
            OnPlayerDisconnectedEvent.Invoke(playerRef);
            if (playerDataManager.HasUser(playerRef))
            {
                playerDataManager.RemovePlayer(playerRef);
                levelResults.Remove(playerRef);
            }
        }

        private void OnLocalUserDataReady()
        {
            RPC_OnUserDataReady(
                playerRef: Runner.LocalPlayer,
                userId: playerDataManager.LocalUserId
            );
        }

        public bool IsPlayerInitialized(PlayerRef playerRef)
        {
            return playerDataManager.HasUser(playerRef);
        }

        public LevelResultsData? GetLevelResults(PlayerRef playerRef)
        {
            if (levelResults.TryGet(playerRef, out var levelResultsData))
            {
                return levelResultsData;
            }

            return null;
        }

        public void OnLevelResultsShown(PlayerRef playerRef)
        {
            levelResults.Remove(playerRef);
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

        public void OnLevelFinished(Dictionary<PlayerRef, LevelResultsData> levelResults)
        {
            this.levelResults.Clear();
            foreach (var (playerRef, levelResultsData) in levelResults)
            {
                if (playerDataManager.HasUser(playerRef))
                {
                    this.levelResults.Add(playerRef, levelResultsData);
                    playerDataManager.RPC_ApplyPlayerRewards(playerRef, levelResultsData);
                }
            }

            LoadLevel(-1);
        }

        private void LoadLevel(int nextLevelIndex)
        {
            if (!HasStateAuthority) return;

            levelTransitionManager.LoadLevel(nextLevelIndex);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPC_OnUserDataReady(PlayerRef playerRef, UserId userId)
        {
            if (playerDataManager.HasUser(userId))
            {
                Debug.Log("Kick player by userId is in game");
                RPC_KickPlayer(playerRef);
                return;
            }
            
            if (levelTransitionManager.CurrentSceneState is SceneState.LEVEL)
            {
                Debug.Log("Kick player by mission is started");
                RPC_KickPlayer(playerRef);
                return;
            }
            
            playerDataManager.AddUserData(playerRef, ref userId);

            playerDataManager.SendAllUsersDataToClient(playerRef);
            playerDataManager.SendAllHeroesDataToClient(playerRef);
            
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