using UnityEngine.UIElements;

namespace Main.Scripts.UI.Gui
{
    public class InteractionInfoView
    {
        private UIDocument doc;

        public InteractionInfoView(UIDocument doc, string keyName, string description)
        {
            this.doc = doc;
            var root = doc.rootVisualElement;
            SetVisibility(false);
            var keyNameLabel = root.Q<Label>("KeyName");
            keyNameLabel.text = $"{keyName}:";
            var descriptionLabel = root.Q<Label>("Description");
            descriptionLabel.text = description;
        }

        public void SetVisibility(bool isVisible)
        {
            doc.rootVisualElement.style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}