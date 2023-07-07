using System.Collections;
using UnityEngine;

namespace Main.Scripts.Utils
{
    public class FpsStatsShower : MonoBehaviour
    {
        private float fps;

        private IEnumerator Start()
        {
            GUI.depth = 2;
            while (true)
            {
                fps = 1f / Time.unscaledDeltaTime;
                yield return new WaitForSeconds(0.1f);
            }
        }
    
        private void OnGUI()
        {
            GUI.Label(new Rect(5, 40, 100, 25), "FPS: " + Mathf.Round(fps));
        }
    }
}