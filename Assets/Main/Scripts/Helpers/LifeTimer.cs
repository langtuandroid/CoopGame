using UnityEngine;

namespace Main.Scripts.Helpers
{
    public class LifeTimer : MonoBehaviour
    {
        [SerializeField]
        private float lifeDurationSec;
        private float spawnTime;

        private void Start()
        {
            spawnTime = Time.time;
        }

        private void Update()
        {
            if (Time.time - spawnTime > lifeDurationSec)
            {
                Destroy(gameObject);
            }
        }

        public void SetLifeDuration(float durationSec)
        {
            lifeDurationSec = durationSec;
        }
    }
}