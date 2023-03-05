using UnityEngine;

namespace Main.Scripts.Settings
{
    public class SettingsManager : MonoBehaviour
    {
        [SerializeField]
        private int lockFPS = 60;

        private void Awake()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = lockFPS;
        }
    }
}