using UnityEngine;
using UnityEngine.Rendering;

namespace Main.Scripts.Helpers
{
    public class RenderersSettingsHelper : MonoBehaviour
    {
        [SerializeField]
        private bool enableShadowCast;

        private void Awake()
        {
            var meshRenderers = GetComponentsInChildren<MeshRenderer>();
            foreach (var meshRenderer in meshRenderers)
            {
                meshRenderer.shadowCastingMode = enableShadowCast ? ShadowCastingMode.On : ShadowCastingMode.Off;
            }
        }
    }
}