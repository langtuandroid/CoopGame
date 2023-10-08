using System;
using Main.Scripts.UI.Windows.DebugPanel.Data;
using UnityEngine.UIElements;

namespace Main.Scripts.UI.Windows.DebugPanel
{
    public class DebugItemViewHolder
    {
        private IntegerField modifierLevelField;
        private ModifierItemData itemData;

        private Action<int>? onChangeValueCallback;

        public DebugItemViewHolder(VisualElement view)
        {
            modifierLevelField = view.Q<IntegerField>("ModifierLevel");
            modifierLevelField.RegisterValueChangedCallback(OnChange);
        }

        public void Bind(ModifierItemData data, Action<int> onChangeValueCallback)
        {
            itemData = data;
            modifierLevelField.label = data.Name + $" MaxLevel={data.MaxLevel}";
            modifierLevelField.value = data.Level;
            this.onChangeValueCallback = onChangeValueCallback;
        }

        private void OnChange(ChangeEvent<int> changeEvent)
        {
            if (changeEvent.newValue > itemData.MaxLevel || changeEvent.newValue < 0)
            {
                modifierLevelField.value = changeEvent.previousValue;
            }
            else
            {
                onChangeValueCallback?.Invoke(changeEvent.newValue);
            }
        }
    }
}