using UnityEngine.UIElements;

namespace Main.Scripts.UI.Windows.HUD.ControlsTextWindow
{
    public class ControlsTextItemViewHolder
    {
        private Label controlsTextName;
        private Label controlsTextDescription;

        public ControlsTextItemViewHolder(VisualElement view)
        {
            controlsTextName = view.Q<Label>("ControlsTextName");
            controlsTextDescription = view.Q<Label>("ControlsTextDescriptions");
        }

        public void Bind(ControlsTextData itemData)
        {
            controlsTextName.text = itemData.ControlKeyName + ":";
            controlsTextDescription.text = itemData.ControlKeyDescription;
        }
    }
}