using System;
using System.Collections.Generic;
using Fusion;
using Main.Scripts.Levels.Results;
using Main.Scripts.Player;
using Main.Scripts.Player.Experience;
using Main.Scripts.Utils;
using Main.Scripts.Utils.Save;
using UnityEngine;

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

        private bool isGameAlreadyRunning;

        [Networked]
        private PlayState networkedPlayState { get; set; }
        [Networked, Capacity(16)]
        private NetworkLinkedList<PlayerRef> connectedPlayers => default;
        [Networked, Capacity(16)]
        private NetworkDictionary<PlayerRef, string> playerNames => default;
        [Networked, Capacity(16)]
        private NetworkDictionary<PlayerRef, PlayerData> playersDataMap => default;
        [Networked, Capacity(16)]
        private NetworkDictionary<PlayerRef, LevelResultsData> levelResults => default;

        public void Awake()
        {
            connectionManager = FindObjectOfType<ConnectionManager>(true).ThrowWhenNull();
            levelTransitionManager = FindObjectOfType<LevelTransitionManager>(true).ThrowWhenNull();
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

                LoadPlayerData();

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
            connectionManager.OnPlayerConnectEvent.RemoveListener(OnPlayerDisconnect);
        }

        private void OnPlayerConnect(NetworkRunner runner, PlayerRef playerRef)
        {
            connectedPlayers.Add(playerRef);
        }

        private void OnPlayerDisconnect(NetworkRunner runner, PlayerRef playerRef)
        {
            connectedPlayers.Remove(playerRef);
        }

        private void LoadPlayerData()
        {
            RPC_InitPlayerData(
                playerRef: Runner.LocalPlayer,
                playerName: connectionManager.PlayerName,
                playerData: SaveLoadUtils.Load(connectionManager.PlayerName)
            );
        }

        public List<PlayerRef> GetConnectedPlayers()
        {
            //copy for safe outside use
            return new List<PlayerRef>(connectedPlayers);
        }

        public PlayerData GetPlayerData(PlayerRef playerRef)
        {
            return playersDataMap.Get(playerRef);
        }

        public void SetPlayerData(PlayerRef playerRef, PlayerData playerData)
        {
            playersDataMap.Set(playerRef, playerData);
            RPC_SavePlayerData(playerRef);
        }

        public LevelResultsData? GetLevelResults(PlayerRef playerRef)
        {
            if (levelResults.TryGet(playerRef, out var levelResultsData))
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

        public void OnLevelFinished(Dictionary<PlayerRef, LevelResultsData> levelResults)
        {
            this.levelResults.Clear();
            foreach (var (playerRef, levelResultsData) in levelResults)
            {
                this.levelResults.Add(playerRef, levelResultsData);
                ApplyPlayerRewards(playerRef, levelResultsData);
            }

            LoadLevel(-1);
        }

        private void LoadLevel(int nextLevelIndex)
        {
            if (!Object.HasStateAuthority)
                return;

            levelTransitionManager.LoadLevel(nextLevelIndex);
        }

        private void ApplyPlayerRewards(PlayerRef playerRef, LevelResultsData levelResultsData)
        {
            var playerData = GetPlayerData(playerRef);
            var experienceForNextLevel = ExperienceHelper.GetExperienceForNextLevel(playerData.Level);
            if (playerData.Experience + levelResultsData.Experience >= experienceForNextLevel)
            {
                playerData.Experience = playerData.Experience + levelResultsData.Experience - experienceForNextLevel;
                playerData.Level = Math.Clamp(playerData.Level + 1, 1, ExperienceHelper.MAX_LEVEL);

                playerData.MaxSkillPoints = ExperienceHelper.GetMaxSkillPointsByLevel(playerData.Level);
            }

            playerData.Experience += levelResultsData.Experience;
            SetPlayerData(playerRef, playerData);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPC_InitPlayerData(PlayerRef playerRef, string playerName, PlayerData playerData)
        {
            playerNames.Set(playerRef, playerName);
            playersDataMap.Set(playerRef, playerData);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_SavePlayerData([RpcTarget] PlayerRef playerRef)
        {
            SaveLoadUtils.Save(connectionManager.PlayerName, GetPlayerData(playerRef));
        }
    }
}