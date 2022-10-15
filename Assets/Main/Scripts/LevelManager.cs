using System.Collections;
using System.Collections.Generic;
using Fusion;
using FusionExamples.FusionHelpers;
using Main.Scripts.Player;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace Main.Scripts
{
    /// <summary>
    /// The LevelManager controls the map - keeps track of spawn points for players and powerups, and spawns powerups at regular intervals.
    /// </summary>
    public class LevelManager : NetworkSceneManagerBase
    {
        [SerializeField]
        private int _lobby;

        [SerializeField]
        private int[] _levels;

        private Scene _loadedScene;

        public FusionLauncher launcher { get; set; }

        private void Awake() { }

        protected override void Shutdown(NetworkRunner runner)
        {
            base.Shutdown(runner);
            if (_loadedScene != default)
            {
                SceneManager.UnloadSceneAsync(_loadedScene);
            }

            _loadedScene = default;
            PlayerManager.ResetPlayerManager();
        }

        // Get a random level
        public int GetRandomLevelIndex()
        {
            int idx = Random.Range(0, _levels.Length);
            // Make sure it's not the same level again. This is partially because it's more fun to try different levels and partially because scene handling breaks if trying to load the same scene again.
            if (_levels[idx] == _loadedScene.buildIndex)
                idx = (idx + 1) % _levels.Length;
            return idx;
        }

        public Vector3 GetPlayerSpawnPoint(int playerID)
        {
            return Vector3.zero;
        }

        public void LoadLevel(int nextLevelIndex)
        {
            Runner.SetActiveScene(nextLevelIndex < 0 ? _lobby : _levels[nextLevelIndex]);
        }

        protected override IEnumerator SwitchScene(SceneRef prevScene, SceneRef newScene,
            FinishedLoadingDelegate finished)
        {
            Debug.Log($"Switching Scene from {prevScene} to {newScene}");
            if (newScene <= 0)
            {
                finished(new List<NetworkObject>());
                yield break;
            }

            if (Runner.IsServer)
            {
                GameManager.playState = GameManager.PlayState.TRANSITION;
            }


            if (prevScene > 0)
            {
                InputController.fetchInput = false;

                for (int i = 0; i < PlayerManager.allPlayers.Count; i++)
                {
                    PlayerManager.allPlayers[i].Despawn();
                }
            }

            launcher.SetConnectionStatus(FusionLauncher.ConnectionStatus.Loading, "");

            yield return null;
            Debug.Log($"Start loading scene {newScene} in single peer mode");

            if (_loadedScene != default)
            {
                Debug.Log($"Unloading Scene {_loadedScene.buildIndex}");
                yield return SceneManager.UnloadSceneAsync(_loadedScene);
            }

            _loadedScene = default;
            Debug.Log($"Loading scene {newScene}");

            List<NetworkObject> sceneObjects = new List<NetworkObject>();
            if (newScene >= 0)
            {
                yield return SceneManager.LoadSceneAsync(newScene, LoadSceneMode.Additive);
                _loadedScene = SceneManager.GetSceneByBuildIndex(newScene);
                Debug.Log($"Loaded scene {newScene}: {_loadedScene}");
                sceneObjects = FindNetworkObjects(_loadedScene, disable: false);
            }

            // Delay one frame
            yield return null;

            launcher.SetConnectionStatus(FusionLauncher.ConnectionStatus.Loaded, "");

            // Activate the next level
            // _currentLevel = FindObjectOfType<LevelBehaviour>(); //todo active level
            // if (_currentLevel != null)
            //     _currentLevel.Activate();

            Debug.Log($"Switched Scene from {prevScene} to {newScene} - loaded {sceneObjects.Count} scene objects");
            finished(sceneObjects);

            SwitchScenePostFadeIn(prevScene, newScene);
        }

        private void SwitchScenePostFadeIn(SceneRef prevScene, SceneRef newScene)
        {
            // Respawn with slight delay between each player
            Debug.Log($"Respawning All Players");
            for (int i = 0; i < PlayerManager.allPlayers.Count; i++)
            {
                PlayerController player = PlayerManager.allPlayers[i];
                Debug.Log($"Respawning Player {i}:{player}");
                player.Respawn(0);
            }

            // Set state to playing level
            if (_loadedScene.buildIndex == _lobby)
            {
                if (Runner.IsServer)
                {
                    GameManager.playState = GameManager.PlayState.LOBBY;
                }

                InputController.fetchInput = true;
                Debug.Log($"Switched Scene from {prevScene} to {newScene}");
            }
            else
            {
                if (Runner != null && (Runner.IsServer))
                {
                    GameManager.playState = GameManager.PlayState.LEVEL;
                }

                // Enable inputs after countdown finishes
                InputController.fetchInput = true;
                Debug.Log($"Switched Scene from {prevScene} to {newScene}");
            }
        }
    }
}