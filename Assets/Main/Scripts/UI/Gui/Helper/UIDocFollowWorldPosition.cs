using Main.Scripts.Utils;
using UnityEngine;
using UnityEngine.UIElements;

namespace Main.Scripts.UI.Gui.Helper
{
    public class UIDocFollowWorldPosition : MonoBehaviour
    {
        [SerializeField]
        private string containerId = default!;

        private UIDocument doc = default!;
        private new Camera camera = default!;

        private VisualElement container = default!;

        private void Awake()
        {
            doc = GetComponent<UIDocument>();
            container = doc.rootVisualElement.Q<VisualElement>(containerId).ThrowWhenNull();
        }

        private void OnEnable()
        {
            camera = Camera.main;
        }

        private void LateUpdate()
        {
            var position = camera.WorldToScreenPoint(transform.position);
            container.style.left = (position.x - Screen.width / 2f);
            container.style.top = (-position.y + Screen.height / 2f);
        }
    }
}