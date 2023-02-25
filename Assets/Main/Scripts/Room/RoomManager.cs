using System.Collections.Generic;
using Fusion;
using Main.Scripts.Levels.Results;
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

        public const ShutdownReason ShutdownReason_GameAlreadyRunning = (ShutdownReason) 100;

        public static RoomManager instance { get; private set; }

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

        private ConnectionManager connectionManager;
        private LevelTransitionManager levelTransitionManager;

        private bool isGameAlreadyRunning;

        [Networked]
        private PlayState networkedPlayState { get; set; }
        [Networked, Capacity(16)]
        private NetworkLinkedList<PlayerRef> connectedPlayers => default;
        [Networked, Capacity(16)]
        private NetworkDictionary<PlayerRef, LevelResultsData> levelResults => default;

        public void Awake()
        {
            connectionManager = FindObjectOfType<ConnectionManager>(true);
            levelTransitionManager = FindObjectOfType<LevelTransitionManager>(true);
        }

        public override void Spawned()
        {
            if (instance)
                Runner.Despawn(Object); // TODO: I've never seen this happen - do we really need this check?
            else
            {
                instance = this;
                DontDestroyOnLoad(this);

                if (Object.HasStateAuthority)
                {
                    connectionManager.OnPlayerConnectEvent.AddListener(OnPlayerConnect);
                    connectionManager.OnPlayerDisconnectEvent.AddListener(OnPlayerDisconnect);
                    LoadLevel(-1);
                }
                else if (playState != PlayState.LOBBY)
                {
                    Debug.Log("Rejecting Player, game is already running!");
                    isGameAlreadyRunning = true;
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

        public List<PlayerRef> GetConnectedPlayers()
        {
            //copy for safe outside use
            return new List<PlayerRef>(connectedPlayers);
        }

        public LevelResultsData? GetLevelResults(PlayerRef playerRef)
        {
            if (levelResults.TryGet(playerRef, out var levelResultsData))
            {
                return levelResultsData;
            }

            return null;
        }

        public void ShutdownRoomConnection(ShutdownReason shutdownReason)
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
            }

            LoadLevel(-1);
        }

        private void LoadLevel(int nextLevelIndex)
        {
            if (!Object.HasStateAuthority)
                return;

            levelTransitionManager.LoadLevel(nextLevelIndex);
        }
    }
}