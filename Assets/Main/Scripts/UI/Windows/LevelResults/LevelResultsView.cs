using Main.Scripts.Player.Data;
using UnityEngine;
using UnityEngine.UIElements;

namespace Main.Scripts.UI.Windows.LevelResults
{
    public class LevelResultsView : MonoBehaviour
    {
        private UIDocument doc = default!;
        private Label levelResultsText = default!;

        private void Awake()
        {
            doc = GetComponent<UIDocument>();
            var root = doc.rootVisualElement;
            SetVisibility(false);
            levelResultsText = root.Q<Label>("LevelResultsText");
        }

        public void SetVisibility(bool isVisible)
        {
            doc.rootVisualElement.style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public void Bind(AwardsData awardsData)
        {
            levelResultsText.text = awardsData.IsSuccess ? "Win" : "Lose";
            levelResultsText.style.color = new StyleColor(awardsData.IsSuccess ? new Color(0, 255, 0) : new Color(255, 0, 0));
        }
    }
}