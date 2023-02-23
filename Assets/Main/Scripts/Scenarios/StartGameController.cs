using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Main.Scripts.Scenarios
{
    public class StartGameController : MonoBehaviour
    {
        private const int MIN_TIME_FOR_LOADING_SHOWING = 1;
        
        [SerializeField]
        private int managerHolderScene;
        [SerializeField]
        private int mainMenuScene;

        private void Start()
        {
            StartCoroutine(LoadGame());
        }

        private IEnumerator LoadGame()
        {
            var startTime = Time.realtimeSinceStartup;
            var currentScene = SceneManager.GetActiveScene();
            yield return SceneManager.LoadSceneAsync(managerHolderScene, LoadSceneMode.Additive);
            yield return SceneManager.LoadSceneAsync(mainMenuScene, LoadSceneMode.Additive);
            var deltaTime = Time.realtimeSinceStartup - startTime;
            if (deltaTime < MIN_TIME_FOR_LOADING_SHOWING)
            {
                yield return new WaitForSeconds(MIN_TIME_FOR_LOADING_SHOWING - deltaTime);
            }
            SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(mainMenuScene));
            yield return SceneManager.UnloadSceneAsync(currentScene);
        }
    }
}