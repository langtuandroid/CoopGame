using System.Collections;
using Cysharp.Threading.Tasks;
using Main.Scripts.Core.Resources;
using Main.Scripts.Utils;
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
            LoadGame();
        }

        private async void LoadGame()
        {
            var startTime = Time.realtimeSinceStartup;
            var currentScene = SceneManager.GetActiveScene();
            await SceneManager.LoadSceneAsync(managerHolderScene, LoadSceneMode.Additive);
            await GlobalResources.Instance.ThrowWhenNull().Init();
            await SceneManager.LoadSceneAsync(mainMenuScene, LoadSceneMode.Additive);
            var deltaTime = Time.realtimeSinceStartup - startTime;
            if (deltaTime < MIN_TIME_FOR_LOADING_SHOWING)
            {
                await UniTask.WaitForSeconds(MIN_TIME_FOR_LOADING_SHOWING - deltaTime);
            }
            SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(mainMenuScene));
            await SceneManager.UnloadSceneAsync(currentScene);
        }
    }
}