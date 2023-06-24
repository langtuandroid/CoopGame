 using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace Main.Scripts.Helpers.Light
{
    public class LightShadowMapHelper : MonoBehaviour
    {
        [SerializeField]
        private float refreshShadowsDeltaTime = 0.5f;
            
        private HDAdditionalLightData lightData = default!;
        private float lastUpdateTime;
        private UnityEngine.Light light = default!;

        private void Awake()
        {
            lightData = GetComponent<HDAdditionalLightData>();
            light = GetComponent<UnityEngine.Light>();
            lastUpdateTime = Time.time;
        }

        private void LateUpdate()
        {
            if (Time.time - lastUpdateTime > refreshShadowsDeltaTime && light.enabled)
            {
                lastUpdateTime = Time.time;
                lightData.RequestShadowMapRendering();
            }
        }
    }
}