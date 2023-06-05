using System;
using Main.Scripts.UI.Windows.Customization.Data;
using UnityEngine.UIElements;

namespace Main.Scripts.UI.Windows.Customization
{
    public class CustomizationItemViewHolder
    {
        private Label nameLabel;
        private Button applyButton;

        private Action? onApplyCallback;

        public CustomizationItemViewHolder(VisualElement view)
        {
            nameLabel = view.Q<Label>("ItemName");
            applyButton = view.Q<Button>("ApplyBtn");
            applyButton.clicked += () => { onApplyCallback?.Invoke(); };
        }

        public void Bind(CustomizationItemData data, Action onApplyCallback)
        {
            nameLabel.text = data.Name;
            applyButton.SetEnabled(!data.IsSelected);
            this.onApplyCallback = onApplyCallback;
        }
    }
}