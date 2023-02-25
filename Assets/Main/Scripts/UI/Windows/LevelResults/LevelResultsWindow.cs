using Main.Scripts.Levels.Results;
using UnityEngine;
using UnityEngine.UIElements;

namespace Main.Scripts.UI.Windows.LevelResults
{
    public class LevelResultsWindow : MonoBehaviour
    {
        private UIDocument doc;
        private Label levelResultsText;

        private void Awake()
        {
            doc = GetComponent<UIDocument>();
            var root = doc.rootVisualElement;
            root.visible = false;
            levelResultsText = root.Q<Label>("LevelResultsText");
        }

        public void SetVisibility(bool isVisible)
        {
            doc.rootVisualElement.visible = isVisible;
        }

        public void Bind(LevelResultsData levelResultsData)
        {
            levelResultsText.text = levelResultsData.IsSuccess ? "Win" : "Lose";
            levelResultsText.style.color = new StyleColor(levelResultsData.IsSuccess ? new Color(0, 255, 0) : new Color(255, 0, 0));
        }
    }
}