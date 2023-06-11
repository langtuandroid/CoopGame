using System;
using Main.Scripts.UI.Windows.DebugPanel.Data;
using UnityEngine.UIElements;

namespace Main.Scripts.UI.Windows.DebugPanel
{
    public class DebugItemViewHolder
    {
        private Toggle ModifierToggle;

        private Action<bool>? onChangeValueCallback;

        public DebugItemViewHolder(VisualElement view)
        {
            ModifierToggle = view.Q<Toggle>("Toggle");
            ModifierToggle.RegisterValueChangedCallback(OnChange);
        }

        public void Bind(ModifierItemData data, Action<bool> onChangeValueCallback)
        {
            ModifierToggle.label = data.Name;
            ModifierToggle.value = data.IsEnabled;
            this.onChangeValueCallback = onChangeValueCallback;
        }

        private void OnChange(ChangeEvent<bool> changeEvent)
        {
            onChangeValueCallback?.Invoke(changeEvent.newValue);
        }
    }
}