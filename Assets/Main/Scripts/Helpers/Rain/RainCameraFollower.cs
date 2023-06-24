using UnityEngine;

namespace Main.Scripts.Helpers.Rain
{
    public class RainCameraFollower: MonoBehaviour
    {
        [SerializeField]
        private float offsetX;
        [SerializeField]
        private float offsetZ;

        private void LateUpdate()
        {
            var targetCamera = Camera.main;
            if (targetCamera != null)
            {
                transform.position = new Vector3(targetCamera.transform.position.x + offsetX, 0, targetCamera.transform.position.z + offsetZ);
            }
        }
    }
}