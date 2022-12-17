using System.Collections;
using System.Collections.Generic;
using Fusion;
using Main.Scripts.FusionHelpers;
using Main.Scripts.Player;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Main.Scripts.Room
{
    public class LevelTransitionManager : NetworkSceneManagerBase
    {
        [SerializeField]
        private int lobby;

        [SerializeField]
        private int[] levels;

        private Scene loadedScene;

        public FusionLauncher launcher { get; set; }

        protected override void Shutdown(NetworkRunner runner)
        {
            base.Shutdown(runner);
            if (loadedScene != default)
            {
                SceneManager.UnloadSceneAsync(loadedScene);
            }

            loadedScene = default;
        }

        public void LoadLevel(int nextLevelIndex)
        {
            Runner.SetActiveScene(nextLevelIndex < 0 ? lobby : levels[nextLevelIndex]);
        }

        protected override IEnumerator SwitchScene(
            SceneRef prevScene,
            SceneRef newScene,
            FinishedLoadingDelegate finished
        )
        {
            Debug.Log($"Switching Scene from {prevScene} to {newScene}");
            if (newScene <= 0)
            {
                finished(new List<NetworkObject>());
                yield break;
            }

            if (Runner.IsServer)
            {
                RoomManager.playState = RoomManager.PlayState.TRANSITION;
            }


            if (prevScene > 0)
            {
                InputController.fetchInput = false;
            }

            launcher.SetConnectionStatus(FusionLauncher.ConnectionStatus.Loading, "");

            yield return null;
            Debug.Log($"Start loading scene {newScene} in single peer mode");

            if (loadedScene != default)
            {
                Debug.Log($"Unloading Scene {loadedScene.buildIndex}");
                yield return SceneManager.UnloadSceneAsync(loadedScene);
            }

            loadedScene = default;
            Debug.Log($"Loading scene {newScene}");

            List<NetworkObject> sceneObjects = new List<NetworkObject>();
            if (newScene >= 0)
            {
                yield return SceneManager.LoadSceneAsync(newScene, LoadSceneMode.Additive);
                loadedScene = SceneManager.GetSceneByBuildIndex(newScene);
                Debug.Log($"Loaded scene {newScene}: {loadedScene}");
                sceneObjects = FindNetworkObjects(loadedScene, disable: false);
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
            if (Runner.IsServer)
            {
                RoomManager.playState = loadedScene.buildIndex == lobby ? RoomManager.PlayState.LOBBY : RoomManager.PlayState.LEVEL;
            }

            InputController.fetchInput = true;
            Debug.Log($"Switched Scene from {prevScene} to {newScene}");
        }
    }
}