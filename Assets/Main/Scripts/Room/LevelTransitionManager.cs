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
        private const int MIN_TIME_FOR_LOADING_SHOWING = 1;

        private static LevelTransitionManager? instance;
        
        [SerializeField]
        private int mainMenuScene;
        [SerializeField]
        private int loadingScene;
        [SerializeField]
        private int lobby;
        [SerializeField]
        private int[] levels;

        private Scene loadedScene;
        private float lastLoadingShowedTime;

        public FusionLauncher launcher { get; set; }

        private void Awake()
        {
            if (instance != null)
            {
                Destroy(this);
                return;
            }

            instance = this;
            DontDestroyOnLoad(this);
            loadedScene = SceneManager.GetActiveScene();
        }

        protected override void Shutdown(NetworkRunner runner)
        {
            base.Shutdown(runner);
            if (loadedScene != default)
            {
                StartCoroutine(LoadMainMenu());
            }
        }

        private IEnumerator LoadMainMenu()
        {
            Debug.Log("LoadMainMenu");
            yield return ShowLoadingScene();
            
            Debug.Log("Unload loaded scene");
            yield return SceneManager.UnloadSceneAsync(loadedScene);
            Debug.Log("Load main menu scene");
            yield return SceneManager.LoadSceneAsync(mainMenuScene, LoadSceneMode.Additive);
            
            yield return HideLoadingScene();
            
            loadedScene = SceneManager.GetActiveScene();
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

            yield return ShowLoadingScene();

            if (loadedScene != default)
            {
                Debug.Log($"Unloading Scene {loadedScene.buildIndex}");
                yield return SceneManager.UnloadSceneAsync(loadedScene);
            }

            loadedScene = default;
            Debug.Log($"Loading scene {newScene}");

            yield return SceneManager.LoadSceneAsync(newScene, LoadSceneMode.Additive);
            loadedScene = SceneManager.GetSceneByBuildIndex(newScene);
            SceneManager.SetActiveScene(loadedScene);
                
            Debug.Log($"Loaded scene {newScene}: {loadedScene}");
            var sceneObjects = FindNetworkObjects(loadedScene, disable: false);

            StartCoroutine(HideLoadingScene());

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

        private IEnumerator ShowLoadingScene()
        {
            Debug.Log("Show loading scene");
            yield return SceneManager.LoadSceneAsync(loadingScene, LoadSceneMode.Additive);
            lastLoadingShowedTime = Time.realtimeSinceStartup;
        }

        private IEnumerator HideLoadingScene()
        {
            Debug.Log("Hide loading scene");
            var deltaTime = Time.realtimeSinceStartup - lastLoadingShowedTime;
            if (deltaTime < MIN_TIME_FOR_LOADING_SHOWING)
            {
                yield return new WaitForSeconds(MIN_TIME_FOR_LOADING_SHOWING - deltaTime);
            }
            yield return SceneManager.UnloadSceneAsync(loadingScene);
        }
    }
}