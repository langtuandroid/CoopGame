using Fusion;
using UnityEngine;
using UnityEngine.AI;

namespace Main.Scripts
{
    public class GameManager : NetworkBehaviour, IStateAuthorityChanged
    {
        public enum PlayState
        {
            LOBBY,
            LEVEL,
            TRANSITION
        }

        [Networked]
        private PlayState networkedPlayState { get; set; }

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

        private LevelManager _levelManager;

        private bool _restart;

        public static GameManager instance { get; private set; }

        public override void Spawned()
        {
            // We only want one GameManager
            if (instance)
                Runner.Despawn(Object); // TODO: I've never seen this happen - do we really need this check?
            else
            {
                instance = this;

                // Find managers and UI
                _levelManager = FindObjectOfType<LevelManager>(true);

                if (Object.HasStateAuthority)
                {
                    NavMesh.avoidancePredictionTime = 0.5f;
                    NavMesh.pathfindingIterationsPerFrame = 500;
                    LoadLevel(-1);
                }
                else if (playState != PlayState.LOBBY)
                {
                    Debug.Log("Rejecting Player, game is already running!");
                    _restart = true;
                }
            }
        }

        public void OnPlayerDeath()
        {
            if (playState != PlayState.LOBBY)
            {
                int playersleft = PlayerManager.PlayersAlive();
                Debug.Log($"Someone died - {playersleft} left");
                if (playersleft == 0)
                {
                    // LoadLevel(nextLevelIndex); //todo
                }
            }
        }

        public void Restart(ShutdownReason shutdownReason)
        {
            if (!Runner.IsShutdown)
            {
                // Calling with destroyGameObject false because we do this in the OnShutdown callback on FusionLauncher
                Runner.Shutdown(false, shutdownReason);
                instance = null;
                _restart = false;
            }
        }

        public const ShutdownReason ShutdownReason_GameAlreadyRunning = (ShutdownReason) 100;

        private void Update()
        {
            if (_restart || Input.GetKeyDown(KeyCode.Escape))
            {
                Restart(_restart ? ShutdownReason_GameAlreadyRunning : ShutdownReason.Ok);
                return;
            }

            PlayerManager.HandleNewPlayers();
        }

        private void ResetStats()
        {
            //todo reset on level loaded
            for (int i = 0; i < PlayerManager.allPlayers.Count; i++)
            {
                // PlayerManager.allPlayers[i].health = MAX_HEALTH;
            }
        }

        private void ResetLives()
        {
            //todo reset on respawn
            for (int i = 0; i < PlayerManager.allPlayers.Count; i++)
            {
                // PlayerManager.allPlayers[i].health = MAX_HEALTH;
            }
        }

        // Transition from lobby to level
        public void OnAllPlayersReady() //todo start level
        {
            Debug.Log("All players are ready");
            if (playState != PlayState.LOBBY)
                return;

            // Reset stats and transition to level.
            ResetStats();

            // close and hide the session from matchmaking / lists. this demo does not allow late join.
            Runner.SessionInfo.IsOpen = false;
            Runner.SessionInfo.IsVisible = false;

            LoadLevel(_levelManager.GetRandomLevelIndex());
        }

        private void LoadLevel(int nextLevelIndex)
        {
            if (!Object.HasStateAuthority)
                return;

            // Reset lives and transition to level
            ResetLives();

            // Reset players ready state so we don't launch immediately
            // for (int i = 0; i < PlayerManager.allPlayers.Count; i++)
            //     PlayerManager.allPlayers[i].ResetReady();


            _levelManager.LoadLevel(nextLevelIndex);
        }

        public void StateAuthorityChanged()
        {
            //todo on host changed
            Debug.Log($"State Authority of GameManager changed: {Object.StateAuthority}");
        }
    }
}