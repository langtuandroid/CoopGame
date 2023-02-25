using System;
using System.Collections.Generic;
using Fusion;
using Main.Scripts.Levels.Results;
using Main.Scripts.Player.Data;
using Main.Scripts.Player.Experience;
using Main.Scripts.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace Main.Scripts.Room
{
    public class RoomManager : NetworkBehaviour
    {
        public enum PlayState
        {
            LOBBY,
            LEVEL,
            TRANSITION
        }

        public const ShutdownReason ShutdownReason_GameAlreadyRunning = (ShutdownReason)100;

        public static RoomManager? instance { get; private set; }

        //todo remove static
        public static PlayState playState
        {
            get => (instance != null && instance.Object != null && instance.Object.IsValid)
                ? instance.networkedPlayState
                : PlayState.LOBBY;
            set
            {
                if (instance != null && instance.Object != null && instance.Object.IsValid)
                    instance.networkedPlayState = value;
            }
        }

        private ConnectionManager connectionManager = default!;
        private LevelTransitionManager levelTransitionManager = default!;
        private PlayerDataManager playerDataManager = default!;

        private bool isGameAlreadyRunning;

        [Networked]
        private PlayState networkedPlayState { get; set; }
        [Networked, Capacity(16)]
        private NetworkDictionary<UserId, PlayerRef> playerRefsMap => default;
        [Networked, Capacity(16)]
        private NetworkDictionary<PlayerRef, UserId> userIdsMap => default;
        [Networked, Capacity(16)]
        private NetworkDictionary<UserId, PlayerData> playersDataMap => default;
        [Networked, Capacity(16)]
        private NetworkDictionary<UserId, LevelResultsData> levelResults => default;

        public UnityEvent<PlayerRef> OnPlayerInitializedEvent = default!;

        public void Awake()
        {
            connectionManager = FindObjectOfType<ConnectionManager>(true).ThrowWhenNull();
            levelTransitionManager = FindObjectOfType<LevelTransitionManager>(true).ThrowWhenNull();
            playerDataManager = FindObjectOfType<PlayerDataManager>().ThrowWhenNull();
        }

        public override void Spawned()
        {
            if (instance)
                Runner.Despawn(Object); // TODO: I've never seen this happen - do we really need this check?
            else
            {
                instance = this;
                DontDestroyOnLoad(this);

                if (playState != PlayState.LOBBY)
                {
                    Debug.Log("Rejecting Player, game is already running!");
                    isGameAlreadyRunning = true;
                    return;
                }

                InitPlayerData();

                if (Object.HasStateAuthority)
                {
                    connectionManager.OnPlayerConnectEvent.AddListener(OnPlayerConnect);
                    connectionManager.OnPlayerDisconnectEvent.AddListener(OnPlayerDisconnect);

                    LoadLevel(-1);
                }
            }
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            connectionManager.OnPlayerConnectEvent.RemoveListener(OnPlayerConnect);
            connectionManager.OnPlayerDisconnectEvent.RemoveListener(OnPlayerDisconnect);
        }

        private void OnPlayerConnect(NetworkRunner runner, PlayerRef playerRef)
        {
            //todo sync in level controller
        }

        private void OnPlayerDisconnect(NetworkRunner runner, PlayerRef playerRef)
        {
            //todo sync in level controller
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

        public void SetPlayerData(UserId userId, PlayerData playerData)
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

        private void ShutdownRoomConnection(ShutdownReason shutdownReason)
        {
            if (!Runner.IsShutdown)
            {
                // Calling with destroyGameObject false because we do this in the OnShutdown callback on FusionLauncher
                Runner.Shutdown(false, shutdownReason);
                instance = null;
                isGameAlreadyRunning = false;
            }
        }

        private void Update()
        {
            //todo реализовать возможность переподключения
            if (isGameAlreadyRunning || Input.GetKeyDown(KeyCode.Backspace))
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
                this.levelResults.Add(userId, levelResultsData);
                ApplyPlayerRewards(userId, levelResultsData);
            }

            LoadLevel(-1);
        }

        private void LoadLevel(int nextLevelIndex)
        {
            if (!Object.HasStateAuthority)
                return;

            levelTransitionManager.LoadLevel(nextLevelIndex);
        }

        private void ApplyPlayerRewards(UserId userId, LevelResultsData levelResultsData)
        {
            var playerData = GetPlayerData(userId);
            var experienceForNextLevel = ExperienceHelper.GetExperienceForNextLevel(playerData.Level);
            if (playerData.Experience + levelResultsData.Experience >= experienceForNextLevel)
            {
                playerData.Experience = playerData.Experience + levelResultsData.Experience - experienceForNextLevel;
                playerData.Level = Math.Clamp(playerData.Level + 1, 1, ExperienceHelper.MAX_LEVEL);

                playerData.MaxSkillPoints = ExperienceHelper.GetMaxSkillPointsByLevel(playerData.Level);
            }

            playerData.Experience += levelResultsData.Experience;
            SetPlayerData(userId, playerData);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPC_InitPlayerData(PlayerRef playerRef, UserId userId, PlayerData playerData)
        {
            playerRefsMap.Set(userId, playerRef);
            userIdsMap.Set(playerRef, userId);
            playersDataMap.Set(userId, playerData);

            OnPlayerInitializedEvent.Invoke(playerRef);
        }
    }
}