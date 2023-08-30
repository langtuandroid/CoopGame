using System.Collections;
using System.Collections.Generic;
using Fusion;
using Main.Scripts.Connection;
using Main.Scripts.Utils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Main.Scripts.Room.Transition
{
    public class LevelTransitionManager : NetworkSceneManagerBase
    {
        private const int MIN_TIME_FOR_LOADING_SHOWING = 1;
        
        public static LevelTransitionManager? Instance { get; private set; }

        [SerializeField]
        private int mainMenuScene;
        [SerializeField]
        private int loadingScene;
        [SerializeField]
        private int lobby;
        [SerializeField]
        private int[] levels = default!;
        private SessionManager sessionManager = default!;

        private Scene activeScene => SceneManager.GetActiveScene();
        private float lastLoadingShowedTime;

        public SceneState CurrentSceneState { get; private set; }
        public UnityEvent<SceneState> OnSceneStateChangedEvent = default!;

        private void Awake()
        {
            Assert.Check(Instance == null);
            Instance = this;
            DontDestroyOnLoad(this);
        }

        private void OnDestroy()
        {
            Instance = null;
        }

        protected override void Initialize(NetworkRunner runner)
        {
            base.Initialize(runner);
            Debug.Log("Initialize");

            sessionManager = SessionManager.Instance.ThrowWhenNull();
            sessionManager.OnConnectionStatusChangedEvent.AddListener(OnConnectionStatusChanged);
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

            UpdateSceneState(SceneState.TRANSITION);

            yield return null;
            Debug.Log($"Start loading scene {newScene} in single peer mode");

            yield return ShowLoadingScene();

            if (activeScene != default)
            {
                Debug.Log($"Unloading Scene {activeScene.buildIndex}");
                yield return SceneManager.UnloadSceneAsync(activeScene);
            }

            Debug.Log($"Loading scene {newScene}");

            yield return SceneManager.LoadSceneAsync(newScene, LoadSceneMode.Additive);
            var loadedScene = SceneManager.GetSceneByBuildIndex(newScene);
            SceneManager.SetActiveScene(loadedScene);

            Debug.Log($"Loaded scene {newScene}: {loadedScene}");
            var sceneObjects = FindNetworkObjects(loadedScene, disable: false);

            StartCoroutine(HideLoadingScene());

            // Delay one frame
            yield return null;

            Debug.Log($"Switched Scene from {prevScene} to {newScene} - loaded {sceneObjects.Count} scene objects");
            finished(sceneObjects);

            SwitchScenePostFadeIn(prevScene, newScene);
        }

        private void SwitchScenePostFadeIn(SceneRef prevScene, SceneRef newScene)
        {
            UpdateSceneState(activeScene.buildIndex == lobby ? SceneState.LOBBY : SceneState.LEVEL);

            Debug.Log($"Switched Scene from {prevScene} to {newScene}");
        }

        private void OnConnectionStatusChanged(ConnectionStatus status)
        {
            if (status == ConnectionStatus.Disconnected)
            {
                if (activeScene.buildIndex != mainMenuScene)
                {
                    StartCoroutine(LoadMainMenu());
                }

                sessionManager.OnConnectionStatusChangedEvent.RemoveListener(OnConnectionStatusChanged);
            }
        }

        private IEnumerator LoadMainMenu()
        {
            Debug.Log("LoadMainMenu");
            yield return ShowLoadingScene();

            Debug.Log("Unload loaded scene");
            yield return SceneManager.UnloadSceneAsync(activeScene);
            Debug.Log("Load main menu scene");
            yield return SceneManager.LoadSceneAsync(mainMenuScene, LoadSceneMode.Additive);
            SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(mainMenuScene));

            yield return HideLoadingScene();
            
            Destroy(this);
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

        private void UpdateSceneState(SceneState sceneState)
        {
            CurrentSceneState = sceneState;
            OnSceneStateChangedEvent.Invoke(sceneState);
        }
    }
}