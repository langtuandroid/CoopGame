using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Main.Scripts.UI.Windows.HUD.ControlsTextWindow
{
    public class ControlsTextView
    {
        private ListView controlsTextListView;
        private VisualTreeAsset controlsTextLayout;
        private List<ControlsTextData> controlsTextItems = new();

        public ControlsTextView(UIDocument doc, VisualTreeAsset controlsTextLayout)
        {
            var root = doc.rootVisualElement;
            this.controlsTextLayout = controlsTextLayout;
            controlsTextListView = root.Q<ListView>("ControlsListView");
        }

        public void Bind(List<ControlsTextData> controlTextList)
        {
            foreach (var item in controlTextList)
            {
                var newItem = new ControlsTextData(item.ControlKeyName, item.ControlKeyDescription);
                controlsTextItems.Add(newItem);
            }
            controlsTextListView.itemsSource = controlsTextItems;

            controlsTextListView.makeItem = () =>
            {
                var itemView = controlsTextLayout.Instantiate();

                var itemViewHolder = new ControlsTextItemViewHolder(itemView);
                itemView.userData = itemViewHolder;

                return itemView;
            };
            controlsTextListView.bindItem = (item, index) =>
            {
                (item.userData as ControlsTextItemViewHolder)?.Bind(
                    controlsTextItems[index]);
            };
        }
    }
}