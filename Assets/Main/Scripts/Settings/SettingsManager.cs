using Fusion;
using UnityEngine;

namespace Main.Scripts.Settings
{
    public class SettingsManager : MonoBehaviour
    {
        public static SettingsManager? Instance { get; private set; }

        [SerializeField]
        private int lockFPS = 60;

        private void Awake()
        {
            Assert.Check(Instance == null);
            Instance = this;

            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = lockFPS;
        }

        private void OnDestroy()
        {
            Instance = null;
        }
    }
}